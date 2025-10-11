namespace TrainCrewTIDWindow.Settings {
    public class NumberImageSetting(string text, int x, int y, int width) {

        public string Text { get; private set; } = text;
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public int Width { get; private set; } = width;
    }
}
