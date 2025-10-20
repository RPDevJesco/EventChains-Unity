using System.Collections.Generic;

public class BTContext
{
    public AIAgent Agent { get; set; }
    public GameState GameState { get; set; }
    public AIState State { get; set; }
    public List<AIAction> Actions { get; set; }
}