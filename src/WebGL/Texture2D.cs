namespace Blazor.WebGL
{
    public class Texture2D
    {
        internal int Id { get; private set; }

        internal Texture2D(int id)
        {
            this.Id = id;
        }
    }
}