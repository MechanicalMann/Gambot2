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

        public Person() { }

        public Person(Person person)
        {
            Id = person.Id;
            Name = person.Name;
            IsActive = person.IsActive;
            IsAdmin = person.IsAdmin;
            Mention = person.Mention;
        }
    }
}