using System.Threading.Tasks;

public abstract class BTNode
{
    public string Name { get; protected set; }
    
    public BTNode(string name)
    {
        Name = name;
    }
    
    public abstract Task<BTNodeResult> Execute(BTContext context);
}