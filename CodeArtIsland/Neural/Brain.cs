using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace OnTheOriginOfFishies.Neural
{
    public class Brain
    {
        public List<Neuron>[] Layers;

        public Brain(int layers)
        {
            Layers = new List<Neuron>[layers];
            for (int i = 0; i < layers; i++)
                Layers[i] = new List<Neuron>();
        }

        public Brain(Brain other) : this(other.Layers.Length)
        {
            var ll = new int[Layers.Length];
            for (int i = Layers.Length - 1; i >= 0; i--)
                for (int j = 0; j < other.Layers[i].Count; j++)
                {
                    var n = new Neuron();
                    n.Bias = other.Layers[i][j].Bias;
                    if (i < Layers.Length - 1)
                    {
                        var r = 0;
                        foreach (var o in Layers[i + 1])
                            n.Axons.Add(new Axon(o, other.Layers[i][j].Axons[r++].Weight));
                    }
                    Layers[i].Add(n);
                }
        }

        public void FullyConnect(params int[] layerSize)
        {
            if (layerSize.Length != Layers.Length)
                throw new Exception();

            for (int i = layerSize.Length - 1; i >= 0; i--)
            {
                for (int j = 0; j < layerSize[i]; j++)
                {
                    var n = new Neuron();
                    if (i < layerSize.Length - 1)
                        foreach (var o in Layers[i + 1])
                            n.Axons.Add(new Axon(o, 0));
                    Layers[i].Add(n);
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < Layers.Length; i++)
                foreach (var n in Layers[i])
                    n.Reset();
        }

        public void Step(params double[] input)
        {
            for (int i = 0; i < input.Length; i++)
                Layers[0][i].Sum += input[i];
            for (int i = 0; i < Layers.Length; i++)
            {
                foreach (var n in Layers[i])
                    n.Activate();
            }
        }
        
        public void Randomize(double weightsAmount)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                foreach (var n in Layers[i])
                {
                    n.Bias += Util.RndDouble(-weightsAmount, weightsAmount);
                    foreach (var a in n.Axons)
                        a.Weight += Util.RndDouble(-weightsAmount, weightsAmount);
                }
            }
        }

        public void WriteWeights(string filename)
        {
            using (var fs = File.Create(filename))
            using (var w = new BinaryWriter(fs))
                for (int i = 0; i < Layers.Length; i++)
                {
                    foreach (var n in Layers[i])
                    {
                        w.Write(n.Bias);
                        foreach (var a in n.Axons)
                            w.Write(a.Weight);
                    }
                }
        }

        public void ReadWeights(string filename)
        {
            using (var fs = File.OpenRead(filename))
            using (var r = new BinaryReader(fs))
                for (int i = 0; i < Layers.Length; i++)
                {
                    foreach (var n in Layers[i])
                    {
                        n.Bias = r.ReadDouble();
                        foreach (var a in n.Axons)
                            a.Weight = r.ReadDouble();
                    }
                }
        }
    }
}
