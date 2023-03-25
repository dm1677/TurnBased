using System;
using System.Collections.Generic;

public class TreeSearchNode
{
    GameState state;
    TreeSearchNode parent;
    AIAction parentAction;
    List<TreeSearchNode> children = new List<TreeSearchNode>();
    Dictionary<int, int> results = new Dictionary<int, int>();

    Random random = new Random();

    int visits = 0;
    List<AIAction> untriedActions;

    public TreeSearchNode(GameState state, TreeSearchNode parent = null, AIAction parentAction = null)
    {
        this.state = state;
        this.parent = parent;
        this.parentAction = parentAction;

        results.Add(1, 0);
        results.Add(-1, 0);

        untriedActions = GetLegalActions(state, User.Player);
    }

    public AIAction GetAction()
    {
        return parentAction;
    }

    TreeSearchNode Expand()
    {
        var action = untriedActions[0];
        untriedActions.RemoveAt(0);
        var nextState = state.Move(action);
        var childNode = new TreeSearchNode(nextState, this, action);
        children.Add(childNode);

        return childNode;
    }

    void BackPropagate(int result)
    {
        visits++;

        if (results.ContainsKey(result))
        {
            results[result]++;
        }

        //results[result]++;
        if (parent != null)
            parent.BackPropagate(result);
    }

    public bool IsFullyExpanded()
    {
        return untriedActions.Count == 0;
    }

    TreeSearchNode BestChild(float cParam = 0.1f)
    {
        double max = 0;
        TreeSearchNode best = null;
        foreach (TreeSearchNode c in children)
        {
            var a = c.q() / c.n();
            var b = a + cParam;
            var i = b * Math.Sqrt((2 * Math.Log(visits)) / c.n());

            if (i > max)
            {
                max = i;
                best = c;
            }
        }
        //Debug(children);
        return best;
    }

    public TreeSearchNode BestAction()
    {
        int simulation = 1;
        
        for (int i = 0; i < simulation; i++)
        {
            var v = TreePolicy();
            var reward = v.Rollout(state);
            v.BackPropagate(reward);
        }

        return BestChild(0);
    }

    void Debug(TreeSearchNode a)
    {
        Godot.Logging.Log(!a.IsFullyExpanded());
    }

    TreeSearchNode TreePolicy()
    {
        TreeSearchNode currentNode = this;
        
        while (!currentNode.IsTerminalNode())
        {
            Godot.Logging.Log("wee");
            if (!currentNode.IsFullyExpanded())
                return currentNode.Expand();
            else
                currentNode = currentNode.BestChild();
        }

        return currentNode;
    }

    public bool IsTerminalNode()
    {
        return state.IsGameOver();
    }

    int n()
    {
        return visits;
    }

    int q()
    {
        var wins = results[1];
        var losses = results[-1];
        return wins - losses;
    }

    public List<AIAction> GetLegalActions(GameState gameState, User player)
    {
        var legalActions = new List<AIAction>();
        int money = -1;

        foreach (UnitState unit in gameState.units)
        {
            if (unit.Owner != player)
                continue;

            if (unit.UnitType == Unit.Resource && unit.Owner == player)
            {
                money = unit.Health;
                continue;
            }

            for (int y = 0; y < gameState.mapHeight; y++)
            {
                for (int x = 0; x < gameState.mapWidth; x++)
                {
                    if (unit.UnitClass.Speed > 0 && gameState.Passable(x, y) && AIMoveAction.CheckMovement(unit.UnitClass.MovementType, gameState, unit, x, y))
                    {
                        legalActions.Add(new AIMoveAction(x, y, unit));
                    }
                }
            }

            if (unit.UnitClass.Damage > 0 && unit.UnitClass.Range > 0)
            {
                var validTargets = AIAttackAction.Attack(unit.UnitClass.AttackType, gameState, unit);

                foreach (UnitState enemyUnit in validTargets)
                    legalActions.Add(new AIAttackAction(unit, enemyUnit));
            }

        }

        if (money == -1) throw new Exception("Undefined resource");

        var values = Enum.GetValues(typeof(Unit));
        foreach (Unit unitType in values) {
            
            if (unitType == Unit.King || unitType == Unit.Resource || unitType == Unit.Tree) continue;

            if (money > 0 && money >= ComponentFactory.Instance().UnitCost(unitType))
                for (int y = 0; y < gameState.mapHeight; y++)
                {
                    for (int x = 0; x < gameState.mapWidth; x++)
                    {
                        if (gameState.Passable(x, y))
                        {
                            legalActions.Add(new AICreateAction(x, y, unitType, player));
                        }
                    }
                }
        }
        return legalActions;
    }

    int Rollout(GameState gameState)
    {
        var currentRolloutState = gameState;
        
        while (!currentRolloutState.IsGameOver())
        {
            var possibleMoves = GetLegalActions(currentRolloutState, User.Player);
            var action = RolloutPolicy(possibleMoves);
            
            currentRolloutState = currentRolloutState.Move(action);
        }

        return 0;//return currentRolloutState.GameResult();
    }

    AIAction RolloutPolicy(List<AIAction> actionList)
    {
        var i = random.Next(actionList.Count);

        return actionList[i];
    }
}