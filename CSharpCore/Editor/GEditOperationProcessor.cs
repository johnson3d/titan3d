using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class GEditOperationProcessor
    {
        public delegate void FSelectedActorsChanged(object sender, List<GamePlay.Actor.GActor> actors);
        public event FSelectedActorsChanged OnSelectedActorsChanged;
        List<GamePlay.Actor.GActor> mSelectedActors = new List<GamePlay.Actor.GActor>();
        public void Cleanup()
        {
            mSelectedActors.Clear();
        }
        public void RaiseOnSelectedActorsChanged(object sender, List<GamePlay.Actor.GActor> actors)
        {
            bool notEqual = false;
            if (mSelectedActors.Count == actors.Count)
            {
                for (int i = 0; i < mSelectedActors.Count; i++)
                {
                    if (mSelectedActors[i] != actors[i])
                    {
                        notEqual = true;
                        break;
                    }
                }
            }
            else
                notEqual = true;
            if(notEqual)
            {
                mSelectedActors = new List<GamePlay.Actor.GActor>(actors);
                OnSelectedActorsChanged(sender, actors);
            }
        }
    }
}
