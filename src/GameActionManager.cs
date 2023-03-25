using Godot;
using System;
using System.Collections.Generic;

public class GameActionManager
{
    private readonly List<Action> actionList = new List<Action>();
    private readonly List<Action> replayList = new List<Action>();
    private Action lastAction;

    public void SaveReplay()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string replayDir = @"\Replays\LastReplay.tbr";
        string path = dir + replayDir;

        System.IO.Directory.CreateDirectory(dir + @"\Replays");

        try
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path))
            {
                foreach (Action action in actionList)
                {
                    var data = action.ReturnData();
                    sw.WriteLine(";");
                    foreach (string s in data)
                        sw.WriteLine(s);
                }
                sw.WriteLine(";");
            }
        }
        catch (Exception e)
        {
            Godot.GD.Print("SerialiseList exception: " + e);
        }
    }

    public void LoadReplay(string path)
    {
        DeserialiseActionList(System.IO.File.ReadLines(path));
    }

    public void RemoveInvalidAction(Action action)
    {
        actionList.Remove(action);
    }

    public void ReplaceLastAction(Action action)
    {
        actionList.RemoveAt(actionList.Count - 1);
        actionList.Add(action);
    }
    
    public void SetReplay()
    {
        replayList.AddRange(actionList);
    }

    public Action GetReplayAction(int index)
    {
        return replayList[index];
    }

    public int GetReplayCount()
    {
        return replayList.Count;
    }

    public int GetActionCount()
    {
        return actionList.Count;
    }

    public void AddActionToList(Action action)
    {
        actionList.Add(action);
    }

    public Action GetLastAction()
    {
        if (actionList.Count == 0) return null;
        return actionList[actionList.Count - 1];
    }

    public void ExecuteLastAction()
    {
        if (actionList.Count > 0)
            GetLastAction().Execute();
    }

    public Action GetUpdatedAction()
    {
        if ((GetActionCount() > 0 && lastAction != GetLastAction())
            || (lastAction == null && GetActionCount() >= 1))
        {
            return GetLastAction();
        }
        return null;
    }

    void DeserialiseActionList(IEnumerable<string> data)
    {
        List<string> actionData = new List<string>();

        foreach (string line in data)
        {
            if (line == ";")
            {
                if (actionData.Count > 0)
                {
                    var action = DeserialiseAction(actionData.ToArray());
                    replayList.Add(action);

                    actionData.Clear();
                }
            }
            else actionData.Add(line);
        }
    }

    Action DeserialiseAction(string[] data)
    {
        var type = Type.GetType(data[0]);
        object[] parameters = new object[data.Length - 1];

        for (int i = 1; i < data.Length; i++)
            parameters[i - 1] = int.Parse(data[i]);

        var action = Activator.CreateInstance(type, parameters);

        return (Action)action;
    }
}