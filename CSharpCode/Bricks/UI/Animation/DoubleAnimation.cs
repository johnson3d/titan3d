using EngineNS.UI.Bind;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Runtime.CompilerServices;

namespace EngineNS.UI.Animation
{
    [BindableObject]
    public partial class DoubleAnimation : AnimationTimeline<double>
    {
        [BindProperty]
        public double From
        {
            get => GetValue<double>();
            set
            {
                SetValue(in value);
            }
        }

        [BindProperty]
        public double To
        {
            get => GetValue<double>();
            set
            {
                SetValue(in value);
            }
        }

        public override void Tick(float elapsedSecond)
        {
            if(CurrentTime > BeginTime)
            {
                var deltaTime = CurrentTime - BeginTime;
                // todo: calculate deltaTime in duration percent
            }
            CurrentTime += TimeSpan.FromSeconds(elapsedSecond);
        }
    }
}

