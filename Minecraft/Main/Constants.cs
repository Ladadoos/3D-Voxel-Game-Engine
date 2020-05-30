namespace Minecraft
{
    static class Constants
    {
        //General
        public const float CUBE_DIM = 1.0F;
        public const float HALF_CUBE_DIM = CUBE_DIM / 2.0F;
        public const int NUM_SECTIONS_IN_CHUNKS = 16;
        public const int MAX_BUILD_HEIGHT = NUM_SECTIONS_IN_CHUNKS * 16;

        //Physics
        public const float GRAVITY = -475F;

        //Player
        public const float PLAYER_HEIGHT = CUBE_DIM * 1.75F;
        public const float PLAYER_CAMERA_HEIGHT = CUBE_DIM * 1.5F;
        public const float PLAYER_WIDTH = HALF_CUBE_DIM; /** X direction */
        public const float PLAYER_LENGTH = HALF_CUBE_DIM; /** Z direction */

        public const float PLAYER_BASE_MOVE_SPEED = 50F;
        public const float PLAYER_SPRINT_MULTIPLIER = 1.75F;
        public const float PLAYER_CROUCH_MULTIPLIER = 0.35F;
        public const float PLAYER_JUMP_FORCE = 115F;
        public const float PLAYER_STOP_FORCE_MULTIPLIER = 0.80F;
        public const float PLAYER_MOUSE_SENSIVITY = 0.0015F;
        public const float PLAYER_IN_AIR_SLOWDOWN = 0.75F;
        public const float PLAYER_FLYING_MULTIPLIER = 16.0F;
    }
}
