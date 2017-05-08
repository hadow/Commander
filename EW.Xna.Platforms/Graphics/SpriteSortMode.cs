
namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public enum  SpriteSortMode
    {
        /// <summary>
        /// All sprites are drawing when (SpriteBatch.End) invokes,in order of draw call sequence.Depth is ignored.
        /// </summary>
        Deferred,
        /// <summary>
        /// Each sprite is drawing at individual draw call
        /// </summary>
        Immediate,//Á¢¼´
        Texture,
        BackToFront,//
        FrontToBack,//
    }
}