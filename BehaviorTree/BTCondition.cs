using System;
using System.Threading.Tasks;

public class BTCondition : BTNode
{
    private readonly Func<BTContext, bool> _condition;
    
    public BTCondition(string name, Func<BTContext, bool> condition) : base(name)
    {
        _condition = condition;
    }
    
    public override Task<BTNodeResult> Execute(BTContext context)
    {
        var result = _condition(context) ? BTNodeResult.Success : BTNodeResult.Failure;
        return Task.FromResult(result);
    }
}