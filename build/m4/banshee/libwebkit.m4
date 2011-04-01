AC_DEFUN([BANSHEE_CHECK_LIBWEBKIT],
[
	PKG_CHECK_MODULES(WEBKITSHARP,
		webkit-sharp-1.0 >= 0.3,
		have_webkit_sharp=yes, have_webkit_sharp=no)
	AC_SUBST(WEBKITSHARP_LIBS)
	AM_CONDITIONAL(HAVE_LIBWEBKIT, [test x$have_webkit_sharp = xyes])
])
