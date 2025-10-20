using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BTSequence : BTNode
{
    private readonly List<BTNode> _children;
    
    public BTSequence(string name, params BTNode[] children) : base(name)
    {
        _children = children.ToList();
    }
    
    public override async Task<BTNodeResult> Execute(BTContext context)
    {
        // Execute each child in order - all must succeed
        foreach (var child in _children)
        {
            var result = await child.Execute(context);
            
            if (result == BTNodeResult.Failure)
                return BTNodeResult.Failure;
            
            if (result == BTNodeResult.Running)
                return BTNodeResult.Running;
        }
        
        return BTNodeResult.Success;
    }
}