using System;
using System.Collections.Generic;
using System.Text;

namespace AEther.WindowsForms
{
    public class RenderBuffer
    {

        public class BufferFinishedEventArgs
        {

            public readonly Model Model;
            public readonly ArraySegment<Instance> Instances;

            public BufferFinishedEventArgs(Model model, ArraySegment<Instance> instances)
            {
                Model = model;
                Instances = instances;
            }

        }

        internal class BufferEntry
        {

            internal readonly Instance[] Instances;
            internal int Position;

            internal BufferEntry(int size)
            {
                Instances = new Instance[size];
                Position = 0;
            }

            internal bool Add(Instance instance)
            {
                Instances[Position] = instance;
                Position += 1;
                if(Position == Instances.Length)
                {
                    Position = 0;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Clear()
            {
                Position = 0;
            }

        }

        public event EventHandler<BufferFinishedEventArgs>? BufferFinished;

        readonly int Size;
        readonly Dictionary<Model, BufferEntry> Entries;

        public RenderBuffer(int size)
        {
            Entries = new Dictionary<Model, BufferEntry>();
            Size = size;
        }

        public void Add(Model model, Instance instance)
        {
            if(!Entries.ContainsKey(model))
            {
                var entry = new BufferEntry(Size);
                Entries[model] = entry;
                entry.Add(instance);
            }
            else
            {
                var entry = Entries[model];
                if(entry.Add(instance))
                {
                    var evt = new BufferFinishedEventArgs(model, entry.Instances);
                    BufferFinished?.Invoke(this, evt);
                }
            }
        }

        public void Clear()
        {
            foreach (var model in Entries.Keys)
            {
                var entry = Entries[model];
                entry.Clear();
            }
        }

        public void Finish()
        {
            foreach(var model in Entries.Keys)
            {
                var entry = Entries[model];
                if(0 < entry.Position)
                {
                    var instances = entry.Instances[0..entry.Position];
                    var evt = new BufferFinishedEventArgs(model, instances);
                    BufferFinished?.Invoke(this, evt);
                }
            }
        }

    }
}
