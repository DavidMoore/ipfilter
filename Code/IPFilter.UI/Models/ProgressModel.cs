namespace IPFilter.UI.Models
{
    public class ProgressModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ProgressModel(UpdateState state, string caption, int value)
        {
            State = state;
            Caption = caption;
            Value = value;
        }

        public UpdateState State { get; set; }
        public string Caption { get; set; }
        public int Value { get; set; }
    }
}