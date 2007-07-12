(let x 15)
(print x)

(set x 10)
(print x)

(let y x)
(print y)

(let 
	(y (* 3 x))
	(z (* 2 y)))

(print "{0}, {1}, {2}" x y z)

(set y (* 2 y))
(print y)

(print "Counting...")

(let i 0)
(while (< i 10) (
	(print i)
	(set i (+ i 1))))

