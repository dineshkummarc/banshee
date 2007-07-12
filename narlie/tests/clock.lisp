;;; loop and .NET method call test

(using 
	system 
	system.threading
)

(while true (
	(writeline console (get_now datetime))
	(sleep thread 1000)
))

