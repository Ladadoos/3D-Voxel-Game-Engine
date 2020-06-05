namespace Minecraft
{
    interface ILightSource
    {
        //The color of the light radiating from this block in all directions.
        //XYB stand for RGB and can have a value between 0 and 15 each
        Vector3i LightColor { get; }
    }
}
