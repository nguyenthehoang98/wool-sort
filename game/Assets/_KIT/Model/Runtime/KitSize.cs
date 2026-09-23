namespace _KIT
{
    public enum KitSize
    {
        Size_2732x2048_4x3 = 0,
        Size_1920x1080_16x9,
        Size_2160x1080_18x9,
        Size_2280x1080_19x9,
        Size_2400x1080_20x9,
        Size_2520x1080_21x9,
        Size_2640x1080_22x9,
        Size_2880x1080_24x9,
    }

    public static class KitSizeConverted
    {
        private static readonly KitEnum<KitOrientation> orientation = new KitEnum<KitOrientation>(
            "orientation",
            KitOrientation.Portrait
        );

        public static KitOrientation Orientation
        {
            get => orientation.Get();
            set => orientation.Set(value);
        }
        
        /// <summary>
        /// width / height
        /// </summary>
        public static float GetRatio(this KitSize size)
        {
            float width = 1;
            float height = 1;

            switch (size)
            {
                case KitSize.Size_2732x2048_4x3:
                    width = 2732;
                    height = 2048;
                    break;
                case KitSize.Size_1920x1080_16x9:
                    width = 1920;
                    height = 1080;
                    break;
                case KitSize.Size_2160x1080_18x9:
                    width = 2160;
                    height = 1080;
                    break;
                case KitSize.Size_2280x1080_19x9:
                    width = 2280;
                    height = 1080;
                    break;
                case KitSize.Size_2400x1080_20x9:
                    width = 2400;
                    height = 1080;
                    break;
                case KitSize.Size_2520x1080_21x9:
                    width = 2520;
                    height = 1080;
                    break;
                case KitSize.Size_2640x1080_22x9:
                    width = 2640;
                    height = 1080;
                    break;
                case KitSize.Size_2880x1080_24x9:
                    width = 2880;
                    height = 1080;
                    break;
            }

            KitOrientation kitOrientation = Orientation;
            float x = kitOrientation == KitOrientation.Landscape ? height : width;
            float y = kitOrientation == KitOrientation.Portrait ? height : width;
            return x / y;
        }
    }
}