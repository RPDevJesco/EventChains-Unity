using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BTSelector : BTNode
{
    private readonly List<BTNode> _children;
    
    public BTSelector(string name, params BTNode[] children) : base(name)
    {
        _children = children.ToList();
    }
    
    public override async Task<BTNodeResult> Execute(BTContext context)
    {
        // Try each child until one succeeds
        foreach (var child in _children)
        {
            var result = await child.Execute(context);
            
            if (result == BTNodeResult.Success)
                return BTNodeResult.Success;
            
            if (result == BTNodeResult.Running)
                return BTNodeResult.Running;
        }
        
        return BTNodeResult.Failure;
    }
}