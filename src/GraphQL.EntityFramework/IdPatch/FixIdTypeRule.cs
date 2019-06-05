using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Validation;

namespace GraphQL.EntityFramework
{
    public class FixIdTypeRule :
        IValidationRule
    {
        static List<IValidationRule> validationRules;
        static NonNullType idNode = new NonNullType(new NamedType(new NameNode("ID")));

        static FixIdTypeRule()
        {
            validationRules = DocumentValidator.CoreRules();
            validationRules.Insert(0, new FixIdTypeRule());
        }

        public INodeVisitor Validate(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<VariableDefinition>(
                    varDefAst =>
                    {
                        if (varDefAst.Name == "id" &&
                            varDefAst.Type is NonNullType nonNullType &&
                            nonNullType.Type is NamedType namedType &&
                            namedType.Name == "String")
                        {
                            varDefAst.Type = idNode;
                        }
                    });
            });
        }

        public static IEnumerable<IValidationRule> CoreRulesWithIdFix => validationRules;
    }
}