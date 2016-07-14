package md59b15f7978abc4d0b2cebe92668d59436;


public class InfoPane_DoubleTapListener
	extends android.view.GestureDetector.SimpleOnGestureListener
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onDoubleTap:(Landroid/view/MotionEvent;)Z:GetOnDoubleTap_Landroid_view_MotionEvent_Handler\n" +
			"";
		mono.android.Runtime.register ("WCCMobile.InfoPane+DoubleTapListener, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", InfoPane_DoubleTapListener.class, __md_methods);
	}


	public InfoPane_DoubleTapListener () throws java.lang.Throwable
	{
		super ();
		if (getClass () == InfoPane_DoubleTapListener.class)
			mono.android.TypeManager.Activate ("WCCMobile.InfoPane+DoubleTapListener, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public boolean onDoubleTap (android.view.MotionEvent p0)
	{
		return n_onDoubleTap (p0);
	}

	private native boolean n_onDoubleTap (android.view.MotionEvent p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
