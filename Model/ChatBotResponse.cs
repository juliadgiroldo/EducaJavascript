public class ChatbotResponse
    {
        public string Texto { get; set; } = "";
        public Dictionary<string, object> Parametros { get; set; } = new();
        public string ContextoDestino { get; set; } = "";
        public int Lifespan { get; set; } = 5;
        public bool ResetarContextos { get; set; } = false;
    }