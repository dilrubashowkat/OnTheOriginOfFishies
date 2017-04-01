using System;
using System.Collections.Generic;

namespace OnTheOriginOfFishies.Neural
{
    public class Neuron
    {
        public enum ActivationType
        {
            HeavysideStep,
            HyperbolicTangent,
            LogisticSigmoid,
        }

        public double Bias;
        public List<Axon> Axons;
        public ActivationType ActivationFct;

        public double Sum;

        public Neuron()
        {
            Axons = new List<Axon>();
            ActivationFct = ActivationType.HyperbolicTangent;
        }

        public void Reset()
        {
            Sum = 0;
        }

        public void Activate()
        {
            double activation = ActivationFunction(Sum, ActivationFct);
            foreach (var c in Axons)
                c.Child.Sum += activation * c.Weight;
        }

        public static double ActivationFunction(double sum, ActivationType type)
        {
            switch (type)
            {
                case ActivationType.HeavysideStep:
                    return sum < 0.0 ? 0.0 : 1.0;
                case ActivationType.HyperbolicTangent:
                    return Math.Tanh(sum);
                case ActivationType.LogisticSigmoid:
                    return 1.0 / (1.0 + Math.Exp(-sum));
                default:
                    throw new Exception("Invalid activation function.");
            }
        }
    }
}
