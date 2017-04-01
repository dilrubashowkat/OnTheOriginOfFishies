namespace OnTheOriginOfFishies.Neural
{
    public class Axon
    {
        public Neuron Child;
        public double Weight;

        public Axon(Neuron child, double weight)
        {
            Child = child;
            Weight = weight;
        }
    }
}
