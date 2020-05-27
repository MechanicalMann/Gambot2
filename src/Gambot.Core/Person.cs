namespace Gambot.Core
{
    public class Person
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsAdmin { get; set; }

        public virtual string Mention { get; set; }

        public override string ToString()
        {
            return Mention;
        }
    }
}