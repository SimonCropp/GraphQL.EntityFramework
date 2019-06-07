using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
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
            Dictionary<string, VariableUsage> variableUsages = null;

            return new EnterLeaveListener(
                listener =>
                {
                    listener.Match<VariableDefinition>(
                        variableDefinition =>
                        {
                            var variableUsage = variableUsages[variableDefinition.Name];
                            if (variableUsage.Type is IdGraphType &&
                                variableDefinition.Type is NonNullType nonNullType &&
                                nonNullType.Type is NamedType namedType &&
                                namedType.Name == "String")
                            {
                                variableDefinition.Type = idNode;
                            }
                        });

                    listener.Match<Operation>(
                        operation =>
                        {
                            variableUsages = context.GetRecursiveVariables(operation)
                                .ToDictionary(x => x.Node.Name, x => x);
                        }
                    );
                });
        }

        public static IEnumerable<IValidationRule> CoreRulesWithIdFix => validationRules;
    }
}