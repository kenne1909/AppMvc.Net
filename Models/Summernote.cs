namespace App.Models{
    public class Summernote
    {
        public Summernote(string idEditor,bool loadLibrary=true)
        {
            IdEditor=idEditor;
            LoadLibrary=loadLibrary;
        }

        public string IdEditor{set;get;}
        public bool  LoadLibrary{set;get;}
        public int  height{set;get;} =120;
        public string toolbar{set;get;} =@"[
          ['style', ['style']],
          ['font', ['bold', 'underline', 'clear']],
          ['color', ['color']],
          ['para', ['ul', 'ol', 'paragraph']],
          ['table', ['table']],
          ['insert', ['link', 'picture', 'video', 'elfinder']],
          ['view', ['fullscreen', 'codeview', 'help']]
        ]";
    }
}