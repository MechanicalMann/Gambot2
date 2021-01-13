namespace Gambot.Module.Factoid
{
    public class Factoid
    {
        public string Trigger { get; set; }
        public string Verb { get; set; }
        public string Response { get; set; }
        public decimal ChanceToTrigger { get; set; }
        public override string ToString() => $"{Trigger} <{Verb}|{ChanceToTrigger}%> {Response}";
    }
}