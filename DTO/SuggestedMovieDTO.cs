using System;

namespace MovieOCD.DTO
{
    public class SuggestedMovieDTO
    {
        public string Title { get; set; }

        public string Year { get; set; }

        protected bool Equals(SuggestedMovieDTO other)
        {
            return string.Equals(Title, other.Title) && string.Equals(Year, other.Year);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Title != null ? Title.GetHashCode() : 0)*397) ^ (Year != null ? Year.GetHashCode() : 0);
            }
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(null, o)) return false;
            if (ReferenceEquals(this, o)) return true;
            return o.GetType() == this.GetType() && Equals((SuggestedMovieDTO) o);
        }
    }
}