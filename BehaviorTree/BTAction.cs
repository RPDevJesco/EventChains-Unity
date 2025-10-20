using System;
using System.Threading.Tasks;

public class BTAction : BTNode
{
    private readonly Func<BTContext, BTNodeResult> _action;
    
    public BTAction(string name, Func<BTContext, BTNodeResult> action) : base(name)
    {
        _action = action;
    }
    
    public override Task<BTNodeResult> Execute(BTContext context)
    {
        var result = _action(context);
        return Task.FromResult(result);
    }
}