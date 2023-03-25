using System;
using System.Collections.Generic;

public class MinimaxNode
{
    GameState State;
    User Player;

    List<AIAction> legalActions = new List<AIAction>();
    List<MinimaxNode> childNodes = new List<MinimaxNode>();

    public float Evaluation { get; private set; }
    public AIAction ParentAction { get; protected set; }

    public static int nodes = 0;

    public MinimaxNode(GameState gameState, User player, AIAction parentAction = null)
    {
        State = gameState;
        Player = player;
        ParentAction = parentAction;

        nodes++;
    }

    float Search(int depth, float alpha, float beta, bool max)
    {
        if (depth == 0) return GetEvaluation();

        if (max) legalActions = AI.GetPrunedActions(State, Player); else PopulateActions();

        if (legalActions.Count == 0) return GetEvaluation();
        if (State.IsGameOver()) return GetEvaluation();

        PopulateChildNodes();

        if (max)
        {
            float best = -1000;
            foreach (MinimaxNode child in childNodes)
            {
                best = Math.Max(best, child.Search(depth - 1, alpha, beta, false));
                alpha = Math.Max(alpha, best);
                if (alpha >= beta) break;
            }
            Evaluation = best;
            return best;
        }
        else
        {
            float best = 1000;
            foreach (MinimaxNode child in childNodes)
            {
                best = Math.Min(best, child.Search(depth - 1, alpha, beta, true));
                beta = Math.Min(beta, best);
                if (beta <= alpha) break;
            }
            Evaluation = best;
            return best;
        }
    }

    public AIAction GetMove(int depth = 2)
    {
        Search(depth, -1000, 1000, true);
        float best = -1000;
        AIAction bestAction = null;
        foreach (MinimaxNode node in childNodes)
        {
            if (node.Evaluation > best)
            {
                best = node.Evaluation;
                bestAction = node.ParentAction;
            }
        }

        Godot.Logging.Log("AI Static Evaluation: " + GetEvaluation());
        Godot.Logging.Log("AI Evaluation: " + best);
        Godot.Logging.Log(nodes + " nodes evaluated");
        Godot.Logging.Log(legalActions.Count + " actions considered");
        //Godot.Logging.Log(state.units.Count + " active entities");
        nodes = 0;

        AI.DebugAction(bestAction);

        return bestAction;
    }

    void PopulateChildNodes()
    {
        foreach (AIAction action in legalActions)
            childNodes.Add(new MinimaxNode(State.Move(action), Owner.SwapPlayers(Player), action));
    }

    void PopulateActions()
    {
        legalActions = AI.GetLegalActions(State, Player);
    }

    void PopulatePrunedActions()
    {
        legalActions = AI.GetPrunedActions(State, Player);
    }

    float GetEvaluation()
    {
        //evaluation = EvaluateResources() + EvaluateUnits() + KingHealthPool();
        int myResources = 0, enemyResources = 0;
        List<UnitState> kings = new List<UnitState>(4);

        foreach (UnitState unit in State.units)
        {
            if (unit.UnitType == Unit.Resource)
            {
                if (unit.Owner == Player)
                    myResources = unit.Health;
                else enemyResources = unit.Health;
            }
            if (unit.UnitType == Unit.King)
                kings.Add(unit);
        }
        
        Evaluation =
              (0.5f * Evaluator.Resources(myResources, enemyResources))
            + (1.5f * Evaluator.UnitCost(State, Player))
            + (14 * Evaluator.KingHealthPool(State, Player))
            + (2 * Evaluator.KingCount(State, Player))
            + (0.5f * Evaluator.Threats(State, Player))
            //+ (2f * KingSafety(myResources, enemyResources))
            + (1.5f * Evaluator.Locality(State, Player, kings))
            // (10 * UnitHealth());
            + (0.1f * Evaluator.KingCrowding(Player, kings))
            + (0.5f * Evaluator.KingTrap(State, Player, kings, myResources, enemyResources));
            //+ (0.3f * Mobility());
            /*
        Evaluation = 
            (Evaluator.KingHealthPool(State, Player)) + 
            Evaluator.Threats(State, Player) +
            Evaluator.UnitCost(State, Player) +
            (Evaluator.UnitS(State, Player));*/

        if (State.GameResult() != User.Neutral)
            Evaluation = (State.GameResult() == Player) ? 1000 : -1000;

        return Evaluation;
    }
}