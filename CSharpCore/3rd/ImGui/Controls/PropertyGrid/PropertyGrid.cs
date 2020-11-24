using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpCode.Controls.PropertyGrid
{
    public class PGCatogoryAttribute : Attribute
    {
        public string Name;
        public PGCatogoryAttribute(string n)
        {
            Name = n;
        }
    }
    public struct Value2
    {
        public int V1 { get; set; }
        public float V2 { get; set; }
        public string V3 { get; set; }
    }

    public class TestTarget
    {
        public string Name { get; set; }
        public float A { get; set; } = 0;
        public int B { get; set; } = 0;
        public class SubClass
        {
            public float A { get; set; } = 1;
            public int B { get; set; } = 1;
            public class SubClass2
            {
                public float A { get; set; } = 2;
                public int B { get; set; } = 2;
                public Value2 C { get; set; }
            }
            public SubClass2 C { get; set; } = new SubClass2();
            public SubClass2 D { get; set; } = new SubClass2();
            public Value2 E { get; set; }
        }
        public SubClass C { get; set; } = new SubClass();
        public SubClass D { get; set; } = new SubClass();
        public TestTarget E { get; set; } = null;
        public Value2 F { get; set; }
    }
    public partial class PropertyGrid
    {
        public static PropertyGrid Test()
        {
            var pg = new PropertyGrid();
            var targets = new List<object>();
            targets.Add(new TestTarget());
            pg.TargetObjects = targets;
            return pg;
        }
        List<object> mTargetObjects;
        public List<object> TargetObjects
        {
            get { return mTargetObjects; }
            set
            {
                mTargetObjects = value;
            }
        }
        public object SingleTarget
        {
            get
            {
                if (mTargetObjects == null || mTargetObjects.Count==0)
                    return null;
                return mTargetObjects[0];
            }
            set
            {
                var targets = new List<object>();
                targets.Add(value);
                TargetObjects = targets;
            }
        }
    }
}
