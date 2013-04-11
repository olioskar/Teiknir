// Teiknir Beta útgáfa 1. 
// Compile skipun: gmcs -pkg:gtk-sharp-2.0 -pkg:mono-cairo teiknirbeta_v1.cs
// Lokaverkefni í viðmótsforritun, teikniforrit.
// Höfundur Ólafur Óskar Egilsson, olioskar@gmail.com
// Dæmahópur 1, kennari Hallgrímur.
//
// Þetta forrit gerir notanda kleyft að teikna ýmis form og fríhendis í ýmsum litum
// á tiltekið svæði.
// 
// Það sem á eftir að útfæra eða fara betur:
// Grípa exceptions í save og load aðgerðum, innsetningu á texta, hreinsa allt takka,
// fyllt form eða bara útlínur. ofl.

using Gtk;
using Cairo;
using System;

public class Draw : Window
{
    // Layout box
    VBox vbox;
    HBox hbox;
    Table tblQuickColors;

    // Takkar
    Button btnFree;
    Button btnRect;
    Button btnElli;	
    Button btnLine;
    Button btnEras;
    Button btnBlue;
    Button btnRed;
    Button btnGreen;
    Button btnYellow;
    Button btnPink;
    Button btnBlack;
    Button btnWhite;
    Button btnPurple;
    Button btnOrange;
    ColorButton btnColor;
    
    Button btnXtraThin;
    Button btnThin;
    Button btnMedium;
    Button btnThick;
    Button btnXtraThick;
    
    Button btnSave;
    Button btnOpen;


    // Litir
    Gdk.Color clrRed = new Gdk.Color (0xff, 0, 0); 
    Gdk.Color clrGreen = new Gdk.Color (0, 0xff, 0); 
    Gdk.Color clrBlue = new Gdk.Color (0, 0, 0xff); 
    Gdk.Color clrYellow = new Gdk.Color (0xff, 0xff, 0); 
    Gdk.Color clrPink = new Gdk.Color (0xff, 0x14, 0x93); 
    Gdk.Color clrPurple = new Gdk.Color (0x80, 0, 0x80); 
    Gdk.Color clrOrange = new Gdk.Color (0xff, 0x45, 0x00); 
    Gdk.Color clrWhite = new Gdk.Color (0xff, 0xff, 0xff); 
    Gdk.Color clrBlack = new Gdk.Color (0, 0, 0); 

    
    // Línuþykkt
    double strokeXtraThin = 0.5;
    double strokeThin = 2;
    double strokeMedium = 3;
    double strokeThick = 4.5;
    double strokeXtraThick = 8;
    double strokeEraser = 30;
    


    // Hér að neðan er nokkurskonar Fastayrðing gagna.
    // elli, rect, line eru teikni aðgerðir sem drawShape getur verið
    delegate void drawShape(Cairo.Context ctx, PointD start, PointD end);
    drawShape painter;
    drawShape elli;
    drawShape rect;
    drawShape line;
    
    // mouseButtonDown er true ef músartakka er haldið niðri, þá eru
    // teikniaðgerðir leyfilegar
    bool mouseButtonDown;
    
    // StartPoints eru upphafspunktar fyrir hverja teikniaðgerð,
    // endPoints eru endapunktar fyrir hverja teikni aðgerð.
    PointD startPoints;
    PointD endPoints;

    // currentStroke er núverandi þykkt á línu í teikniaðgerð hverju sinni
    double currentStroke;
    // currentColor er núverandi litur í teikniaðgerð hverju sinni
    Gdk.Color currentColor;
   
    // freeHandCtx er virkur þegar freeHandMode er true og er
    // til að gera notanda kleyft að teikna fríhendis á teikniborðið
    public Cairo.Context freehandCtx;
    bool freehandMode;
    // ef eraserMode er true þá hefur notandi valið að stroka út
    bool eraserMode;
	
    // Pixbuf geymir í minni mynd af því sem teiknað hefur verið
    // g er grafískur hlutur sem þarf að hafa til viðmiðunar þegar sótt er úr pixbuf
    // Pixbuf inniheldur það sem er í canvas.GdkWindow eftir hverja aðgerð
    // pixbuf er teiknaður inní canvas í ExposeEvent á undan öllum
    // öðrum aðgerðum, þannig viðheld ég því sem teiknað hefur verið.
    // Pixbuf er uppfært í hvert sinn sem músarhnappi er sleppt.
    Gdk.Pixbuf pixbuf;
    Gdk.GC g;
    
    // Canvas er svæðið sem teikna skal á
    DrawingArea canvas;

    public Draw() : base("Teiknir")
    {
	canvas = new DrawingArea();
	mouseButtonDown = false;
	freehandMode = true;
	eraserMode = false;
		
	elli = new drawShape(drawEllipse);
	rect = new drawShape(drawRectangle);
	line = new drawShape(drawLine);
	painter = line;
        currentStroke = strokeThin;
        currentColor = clrBlack;	
	canvas.ExposeEvent += canvasExposed;
		
        //Músa Atburðir
	canvas.AddEvents((int)Gdk.EventMask.PointerMotionMask
			|(int)Gdk.EventMask.ButtonPressMask
			|(int)Gdk.EventMask.ButtonReleaseMask);
		
	canvas.ButtonPressEvent += onMousePress;
	canvas.ButtonReleaseEvent += onMouseRelease;
	canvas.MotionNotifyEvent += onMouseMotion;
	// ---

	DeleteEvent += delegate { Application.Quit();};
	// Smíða tækjastiku	
	btnFree = new Button("Freehand");
	btnRect = new Button("Rectangle");
	btnElli = new Button("Ellipse");
	btnLine = new Button("Line");
	btnEras = new Button("Eraser");
        //btnText = new Button("Text");
	  
	tblQuickColors = new Table(3,3,true);

	btnBlack = new Button();	
	btnWhite = new Button();
	btnYellow = new Button();
		
        modClrBtn(btnBlack,clrBlack);
        modClrBtn(btnWhite,clrWhite);
        modClrBtn(btnYellow,clrYellow);
		
        tblQuickColors.Attach(btnBlack,0,1,0,1);
	tblQuickColors.Attach(btnWhite,1,2,0,1);
	tblQuickColors.Attach(btnYellow,2,3,0,1);
		
	btnBlue = new Button();
	btnGreen = new Button();
	btnRed = new Button();
        
        modClrBtn(btnBlue,clrBlue);
        modClrBtn(btnGreen,clrGreen);
        modClrBtn(btnRed,clrRed);
		
	tblQuickColors.Attach(btnBlue,0,1,1,2);
	tblQuickColors.Attach(btnGreen,1,2,1,2);
	tblQuickColors.Attach(btnRed,2,3,1,2);

	btnPink = new Button();
	btnPurple = new Button();
	btnOrange = new Button(" ");
		
        modClrBtn(btnPink,clrPink);
        modClrBtn(btnPurple,clrPurple);
        modClrBtn(btnOrange,clrOrange);
		
        tblQuickColors.Attach(btnPink,0,1,2,3);
	tblQuickColors.Attach(btnPurple,1,2,2,3);
	tblQuickColors.Attach(btnOrange,2,3,2,3);
        
        btnColor = new ColorButton();
        btnXtraThin = new Button("Extra Thin");
        btnThin = new Button("Thin");
        btnMedium = new Button("Medum");
        btnThick = new Button("Thick");
        btnXtraThick = new Button("Extra Thick");
        
        btnSave = new Button("Save");
        btnOpen = new Button("Open");
    	hbox = new HBox(false,5);
	vbox = new VBox();
        
        addLabel(vbox,"Tools");
        vbox.PackStart(btnFree,false,true,0);
	vbox.PackStart(btnRect,false,true,0);
	vbox.PackStart(btnElli,false,true,0);
	vbox.PackStart(btnLine,false,true,0);
        vbox.PackStart(btnEras,false,true,0);
    	
        addLabel(vbox,"Color");
        vbox.PackStart(btnColor,false,true,0);
        
        addLabel(vbox,"Quick Color");       
        vbox.PackStart(tblQuickColors,false,true,0);
        
        addLabel(vbox,"Stroke");
        vbox.PackStart(btnXtraThin,false,true,0);
        vbox.PackStart(btnThin,false,true,0);
        vbox.PackStart(btnMedium,false,true,0);
        vbox.PackStart(btnThick,false,true,0);
        vbox.PackStart(btnXtraThick,false,true,0);
        
        addLabel(vbox,"File");
        
        vbox.PackStart(btnSave,false,true,0);
        vbox.PackStart(btnOpen,false,true,0);
        
        hbox.PackStart(vbox,false,false,5);
	hbox.Add(canvas);
		
	canvas.HeightRequest = 600;
	canvas.WidthRequest = 800;
		
        //Button Actions!
        btnFree.Clicked += delegate {setMode("freehand");};
        btnLine.Clicked += delegate {setMode("line");};
        btnRect.Clicked += delegate {setMode("rectangle");};
        btnElli.Clicked += delegate {setMode("ellipse");};
        btnEras.Clicked += delegate {setMode("eraser");};
       
        btnColor.ColorSet += delegate {currentColor = btnColor.Color;}; 
        btnBlack.Clicked += delegate {setColor(clrBlack);};
        btnWhite.Clicked += delegate {setColor(clrWhite);};
        btnYellow.Clicked += delegate {setColor(clrYellow);};
        btnPurple.Clicked += delegate {setColor(clrPurple);};
        btnPink.Clicked += delegate {setColor(clrPink);};
        btnBlue.Clicked += delegate {setColor(clrBlue);};
        btnGreen.Clicked += delegate {setColor(clrGreen);};
        btnOrange.Clicked += delegate {setColor(clrOrange);};
        btnRed.Clicked += delegate {setColor(clrRed);};

        btnXtraThin.Clicked += delegate {currentStroke = strokeXtraThin;};
        btnThin.Clicked += delegate {currentStroke = strokeThin;};
        btnMedium.Clicked += delegate {currentStroke = strokeMedium;};
        btnThick.Clicked += delegate {currentStroke = strokeThick;};
        btnXtraThick.Clicked += delegate {currentStroke = strokeXtraThick;};
        
	btnOpen.Clicked += delegate {loadFile();};
        btnSave.Clicked += delegate {saveFile();};
        SetPosition(WindowPosition.Center);
	Add(hbox);
	ShowAll();
	Resizable = false;
       
     // freehand teikni aðferðin þarf sér meðferð (og sér context)
        freehandCtx = Gdk.CairoHelper.Create(canvas.GdkWindow);
        Gdk.CairoHelper.SetSourceColor(freehandCtx, currentColor);
        
        fillAll(clrWhite);
    }
    
    // n: setColor(clr);
    // e: clr er sá litur sem teikna skal með
    private void setColor(Gdk.Color clr)
    {
	currentColor = clr;
	btnColor.Color = clr;
    }
    // n: loadFile(); btnOpen.Clicked kallar á þetta fall
    // f: pixbuf er tilbúinn;
    // e: búið er að opna myndaskrá og setja inní pixbuf
    private void loadFile()
    {
		fillAll(clrWhite);
		Gtk.FileChooserDialog fc =
		new Gtk.FileChooserDialog("Choose the file to open",
		                            this,
		                            FileChooserAction.Open,
		                            "Cancel",ResponseType.Cancel,
		                            "Open",ResponseType.Accept);

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			System.IO.FileStream file=System.IO.File.OpenRead(fc.Filename);
			using (Gdk.Pixbuf pb = new Gdk.Pixbuf(fc.Filename)) 
			{
				pixbuf = pb.Copy();
			}
			file.Close();
		}
		fc.Destroy();
    }
    
    // n: saveFile(); btnSave.Clicked kallar á þetta fall
    // f: pixbuf er til;
    // e: innihald pixbuf hefur verið vistuð sem .png skrá
    private void saveFile()
{
		Gtk.FileChooserDialog fc =
		new Gtk.FileChooserDialog("Choose the file to open",
		                            this,
		                            FileChooserAction.Save,
		                            "Cancel",ResponseType.Cancel,
		                            "Save",ResponseType.Accept);

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			pixbuf.Save(fc.Filename,"png");
		}
		fc.Destroy();
    }

    // n: setMode(teikniaðferð);
    // kallað er á setMode í hvert sinn sem notandi velur nýja teikniaðferð
    // f: canvas er tilbúinn
    // e: Teikniaðferð hefur verið valin  
    private void setMode(string mode)
    {   
        eraserMode = false;
        freehandMode = false;
        if (String.Compare(mode,"freehand")==0) freehandMode = true;
        if (String.Compare(mode,"rectangle")==0) painter = rect;
        if (String.Compare(mode,"ellipse")==0) painter = elli;
        if (String.Compare(mode,"line")==0) painter = line;
        if (String.Compare(mode,"eraser")==0) setEraserMode(true);
    }
 
    // n: fillAll(color);
    // f: canvas er tilbúinn
    // e: canvas.GdkWindow hefur verið hreinsaður og er á litinn eins og color 
    private void fillAll(Gdk.Color clr)
    {
	//pixbuf.Fill(clr);
        using (Cairo.Context ctx = Gdk.CairoHelper.Create(canvas.GdkWindow))
        {
            Gdk.CairoHelper.SetSourceColor(ctx,clr);
            Gdk.CairoHelper.Rectangle(ctx,new Gdk.Rectangle(0,0,
                                              canvas.GdkWindow.FrameExtents.Width,
                                              canvas.GdkWindow.FrameExtents.Height));
            ctx.Fill();
            pixbuf = Gdk.Pixbuf.FromDrawable(canvas.GdkWindow,
					     Gdk.Colormap.System,0,0,0,0,
                                             canvas.GdkWindow.FrameExtents.Width,
                                             canvas.GdkWindow.FrameExtents.Height);
        }
    }

    // n: modClrBtn(takki,lit);
    // f: 
    // e: lit á takka hefur verið breitt 
    private static void modClrBtn(Button btn, Gdk.Color clr)
    {
        btn.ModifyBg(StateType.Normal,clr);
        btn.ModifyBg(StateType.Prelight,clr);
        btn.ModifyBg(StateType.Active,clr);

    }
    // n: addLabel(vb,lbl);
    // e: Labeli með titilinn lbl hefur verið bætt við vb
    private void addLabel (VBox vb, string lbl)
    {
        HSeparator hs1 = new HSeparator();
        HSeparator hs2 = new HSeparator();
        hs1.HeightRequest = 10;
        hs2.HeightRequest = 2;
        Label l = new Label(lbl);
        vb.PackStart(hs1,false,true,0);
        vb.PackStart(l,false,true,0);
        vb.PackStart(hs2,false,true,0);
    }
	
    // Höndla mouse events:

    // e: núverandi teikniaðferð hefur verið gerð virk	
    private void onMousePress(object o, ButtonPressEventArgs args)
    {		   
 	// hreinsa út freehandCtx svo næst þegar teiknað er fríhendis 
        // þá er byrjað á tómum context.
        if (freehandMode) 
        {
            freehandCtx = Gdk.CairoHelper.Create(canvas.GdkWindow);
            if (eraserMode) Gdk.CairoHelper.SetSourceColor(freehandCtx,clrWhite);
            else Gdk.CairoHelper.SetSourceColor(freehandCtx,currentColor);
        }
        startPoints.X = args.Event.X;
        startPoints.Y = args.Event.Y;
        mouseButtonDown = true;
    }
    
    // e: núverandi teikniaðferð er óvirk
    //    og það sem er í canvas.GdkWindow hefur verið vistað í minni.
    private void onMouseRelease(object o, ButtonReleaseEventArgs args)
    {
	mouseButtonDown = false;
        pixbuf = Gdk.Pixbuf.FromDrawable(canvas.GdkWindow,
			                 Gdk.Colormap.System,0,0,0,0,
                                         canvas.GdkWindow.FrameExtents.Width,
                                         canvas.GdkWindow.FrameExtents.Height);
    }
	
    // e: gögn fyrir teikniaðferðir hafa verið uppfærð
    // og skipun um að uppfæra canvas.GdkWindow hefur verið send.
    private void onMouseMotion(object o, MotionNotifyEventArgs args)
    {
	if (!mouseButtonDown) return; 
        endPoints.X = args.Event.X;
        endPoints.Y = args.Event.Y;
        
        // Sérmeðferð ef teiknað er fríhendis
        if (freehandMode && mouseButtonDown)
        {   
            if (eraserMode) freehandCtx.LineWidth = strokeEraser;
            else freehandCtx.LineWidth = currentStroke;
            freehandCtx.Save();
            freehandCtx.MoveTo(startPoints);
            freehandCtx.LineTo(endPoints);
            startPoints = endPoints;
            freehandCtx.Restore();
            freehandCtx.StrokePreserve();
            // Einnig hægt að nota 
            //freehandCtx.Stroke(); 
            // en StrokePreserve() gefur flottari línur
            // Örlítið hakk! En lagar leiðinlegan flicker bug
            // í staðin fyrir að nota QueueDraw().
            // Hinsvegar þá virkar ekki að nota ProcessUpdates
            // í öðrum teikniaðferðum.
            canvas.GdkWindow.ProcessUpdates(false);
        }
        if (!freehandMode) canvas.QueueDraw();
    }	
    
    // n: setEraserMode(o);
    // f: canvas er tilbúinn
    // e: eraserMode er annaðhvort af eða á.
    public void setEraserMode(bool on)
    {
        if (on)
        {
            freehandMode = true;
            eraserMode = true;
        } else
        {
            freehandMode = false;
            eraserMode = false;
        }
    }
    //teikniföll!
    
    // n: drawLine(ctx, start, end);
    // f: canvas er tilbúinn
    // e: teiknuð hefur verið lína í ctx frá start til end
    public void drawLine(Cairo.Context ctx, PointD start, PointD end)
    { 
        ctx.LineWidth = currentStroke;
        ctx.Save();
        ctx.MoveTo(start);
        ctx.LineTo(end);
        ctx.Restore();
        ctx.Stroke();
    }
    
    // n: drawRectangle(ctx, start, end);
    // f: canvas er tilbúinn
    // e: teiknaður hefur verið kassi í ctx frá mótstæðum hornapunktum start til end
    public void drawRectangle(Cairo.Context ctx, PointD start, PointD end)
    {
        ctx.LineWidth = currentStroke;
        double width = end.X - start.X;
        double height = end.Y - start.Y;
        ctx.Save();
        ctx.Rectangle(start, width, height);
        ctx.Restore();
        ctx.Fill();
    }
    
    // n: drawEllipse(ctx, start, end);
    // f: canvas er tilbúinn
    // e: teiknaður hefur verið sporbaugur í ctx frá mótstæðum hornapunktum start til end
    public void drawEllipse(Cairo.Context ctx, PointD start, PointD end)
    {
        double width = Math.Abs(start.X - end.X);
        double height = Math.Abs(start.Y - end.Y);
        double xcenter = start.X + (end.X - start.X) / 2.0;
        double ycenter = start.Y + (end.Y - start.Y) / 2.0;
        ctx.LineWidth = currentStroke;
        ctx.Save();
        ctx.Translate(xcenter, ycenter);
        ctx.Scale(width/2.0, height/2.0);
        ctx.Arc(0.0,0.0,1.0,0.0, 2*Math.PI);
        ctx.Restore();
        ctx.Fill(); 
    }
    // n: canvasExposed(o,args);
    // f: canvas er tilbúinn
    // e: canvas (teiknigluggi) hefur verið uppfærður og 
    private void canvasExposed(object o, ExposeEventArgs args)
    {
	Console.WriteLine("í canvas");
	// Teikna pixbuf í canvasinn
	canvas.GdkWindow.DrawPixbuf (g,pixbuf,0,0,0,0,-1,-1,Gdk.RgbDither.Normal,10,10);
       
        //Sér til þess að ekkert sé teiknað nema músartakki sé niðri. 
        if (!mouseButtonDown) return;
        if (!freehandMode)
        {    
            using(Cairo.Context c = Gdk.CairoHelper.Create(canvas.GdkWindow))
            {
                Gdk.CairoHelper.SetSourceColor(c,currentColor);
                painter(c,startPoints,endPoints);
            }
        }
    }
    // Aðalfall!
    // e: Forritið hefur verið ræst
    public static void Main()
    {
	Application.Init();
	new Draw();
	Application.Run();
    }
}
