namespace Minecraft
{
    class Constants
    {

        /*
         * General
         */

        public const int SECTION_HEIGHT = 16;
        public const float CUBE_SIZE = 1.0F;
        public const float HALF_CUBE_SIZE = CUBE_SIZE / 2.0F;
        public const int CHUNK_SIZE = 16;
        public const int SECTIONS_IN_CHUNKS = 16;
        public const int MAX_BUILD_HEIGHT = SECTIONS_IN_CHUNKS * SECTION_HEIGHT;

        /*
         * Physics
         */

        public const float GRAVITY = -7.25F;
        public const float GRAVITY_THRESHOLD = -12.5F;

        /*
         * Player
         */

        public const int PLAYER_RENDER_DISTANCE = 5;

        public const float PLAYER_HEIGHT = CUBE_SIZE * 1.75F;
        public const float PLAYER_CAMERA_HEIGHT = CUBE_SIZE * 1.5F;
        public const float PLAYER_WIDTH = HALF_CUBE_SIZE; /** X direction */
        public const float PLAYER_LENGTH = HALF_CUBE_SIZE; /** Z direction */

        public const float PLAYER_BASE_MOVE_SPEED = 0.70F;
        public const float PLAYER_SPRINT_MULTIPLIER = 1.75F;
        public const float PLAYER_CROUCH_MULTIPLIER = 0.35F;
        public const float PLAYER_JUMP_FORCE = 2.15F;
        public const float PLAYER_STOP_FORCE_MULTIPLIER = 0.80F;
        public const float PLAYER_MOUSE_SENSIVITY = 0.0015F;
        public const float PLAYER_IN_AIR_SLOWDOWN = 0.75F;
    }
}
