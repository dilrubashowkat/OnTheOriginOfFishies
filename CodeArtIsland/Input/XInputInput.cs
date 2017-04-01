namespace OnTheOriginOfFishies.Input
{
    public static class XInputInput
    {
        public static SharpDX.XInput.Gamepad[] State { get; private set; }
        public static SharpDX.XInput.Gamepad[] PrevState { get; private set; }
        private static SharpDX.XInput.Controller[] controller;

        public static void Init()
        {
            controller = new SharpDX.XInput.Controller[4];
            for (int i = 0; i < controller.Length; i++)
                controller[i] = new SharpDX.XInput.Controller((SharpDX.XInput.UserIndex)i);
            State = new SharpDX.XInput.Gamepad[controller.Length];
            PrevState = new SharpDX.XInput.Gamepad[controller.Length];
        }

        public static void Update()
        {
            for (int i = 0; i < State.Length; i++)
            {
                PrevState[i] = State[i];
                if (controller[i].IsConnected)
                    State[i] = controller[i].GetState().Gamepad;
            }
        }
    }
}
