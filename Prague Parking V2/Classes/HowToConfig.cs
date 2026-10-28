namespace Prague_Parking_V2
{
    public record HowToConfig
    {
        public string[] OpeningTitle { get; } =
        {
                "Here you will learn how to change the configs for the garage",
                "How to change prices and other settings"
        };
        public string[] HowTo { get; } =
        {
                "In the VehicleConfig file you can see currently allowed vehicles to park, their prices and their size.",
                "All of these can be changed but i strongly recommend only Price and size be changed.",
                "Note that changing size can cause errors in the program ",
        };

        private string Joiner(string[] s) => string.Join(Environment.NewLine, s);
        public string SendOpeningTitle() => Joiner(OpeningTitle);
        public string SendHowTo() => Joiner(HowTo);
    }
}

