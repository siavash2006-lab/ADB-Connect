namespace ADB_Connect;

// Shared, scrollable result/confirmation window. Native file pickers keep the Windows theme.
internal sealed class AppDialog : Form
{
    internal AppDialog(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        Text=string.IsNullOrWhiteSpace(caption)?"ADB Connect":caption;
        StartPosition=FormStartPosition.CenterParent; MinimizeBox=false; MaximizeBox=false;
        ShowInTaskbar=false; ClientSize=new Size(560,300); MinimumSize=new Size(400,240);
        var content=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill,Text=text,BorderStyle=BorderStyle.None};
        var body=new Panel{Dock=DockStyle.Fill,Padding=new Padding(16)};
        body.Controls.Add(content);
        var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=52,Padding=new Padding(8),FlowDirection=FlowDirection.RightToLeft};
        Button Add(string label,DialogResult result)
        {
            var button=new Button{Text=label,DialogResult=result,Size=new Size(96,30),Tag=result==DialogResult.OK?"primary":null};
            footer.Controls.Add(button); return button;
        }
        if(buttons==MessageBoxButtons.YesNo)
        {
            var no=Add("No",DialogResult.No); Add("Yes",DialogResult.Yes); AcceptButton=no; CancelButton=no;
            FormClosing+=(_,_)=>{if(DialogResult==DialogResult.Cancel)DialogResult=DialogResult.No;};
        }
        else if(buttons==MessageBoxButtons.OKCancel)
        {
            CancelButton=Add("Cancel",DialogResult.Cancel); AcceptButton=Add("OK",DialogResult.OK);
        }
        else {var ok=Add("OK",DialogResult.OK); AcceptButton=ok; CancelButton=ok;}
        if(icon!=MessageBoxIcon.None)
        {
            var heading=new Label{Text=icon==MessageBoxIcon.Error?"Operation failed":icon==MessageBoxIcon.Warning?"Please confirm":"Information",Dock=DockStyle.Top,Height=32,Padding=new Padding(16,8,0,0)};
            Controls.Add(heading);
        }
        Controls.Add(body); Controls.Add(footer); body.BringToFront();
        AppTheme.Attach(this);
    }
    public static DialogResult Show(string text,string caption="ADB Connect",MessageBoxButtons buttons=MessageBoxButtons.OK,MessageBoxIcon icon=MessageBoxIcon.None)
        => Show(Form.ActiveForm,text,caption,buttons,icon);
    public static DialogResult Show(IWin32Window? owner,string text,string caption="ADB Connect",MessageBoxButtons buttons=MessageBoxButtons.OK,MessageBoxIcon icon=MessageBoxIcon.None)
    {
        using var dialog=new AppDialog(text,caption,buttons,icon);
        return owner==null?dialog.ShowDialog():dialog.ShowDialog(owner);
    }
}
