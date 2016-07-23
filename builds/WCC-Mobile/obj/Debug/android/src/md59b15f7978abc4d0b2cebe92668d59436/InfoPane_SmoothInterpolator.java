package md59b15f7978abc4d0b2cebe92668d59436;


public class InfoPane_SmoothInterpolator
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.animation.TimeInterpolator
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_getInterpolation:(F)F:GetGetInterpolation_FHandler:Android.Animation.ITimeInterpolatorInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("WCCMobile.InfoPane+SmoothInterpolator, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", InfoPane_SmoothInterpolator.class, __md_methods);
	}


	public InfoPane_SmoothInterpolator () throws java.lang.Throwable
	{
		super ();
		if (getClass () == InfoPane_SmoothInterpolator.class)
			mono.android.TypeManager.Activate ("WCCMobile.InfoPane+SmoothInterpolator, WCCMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public float getInterpolation (float p0)
	{
		return n_getInterpolation (p0);
	}

	private native float n_getInterpolation (float p0);

	java.util.ArrayList refList;
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
