This is a parallel conjugate gradients program for distributed memory systems. It can be compiled with any C++ compiler that supports OpenMP and Message Passing Interface, for example:

* mpic++ ConjugateGradients.cpp -o ConjugateGradients

The only accepted command line argument is the task file name (can be generated with task generator). The task file has the following format:

* The first line contains the number of the equations.
* All the other lines excepth the last two contain the coefficients of the system. **The coefficient matrix of the system should be positively defined and symmetric!**
* The next line contains the initial approximation
* The last line contains the acceptable error.