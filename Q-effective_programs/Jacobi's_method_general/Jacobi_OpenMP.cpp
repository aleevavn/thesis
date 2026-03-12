#include <omp.h>
#include <vector>
#include <cstdlib>
#include <iostream>
#include <math.h>
#include <ctime>
#include <fstream>
#include <q_openmp.h>

void Equations::randomInit()
{
srand(time(0));
	for (int i=0;i<size;i++)
	{
		b[i]=rand();
		for (int j=0;j<size;j++)
		{
			A[i][j]=rand();
		}
	}
}

void Equations::keyboardInit()
{
	ifstream A_in, b_in;
	A_in.open("A.input");
	b_in.open("b.input");
	for (int i = 0; i < size; i++)
		b_in >> b[i];
		for (int j = 0; j < size; j++)
		{
			A_in >> A[i][j];
		}
	for (int i = 0; i < size; i++)
		b[i]=1;
		for (int j = 0; j < size; j++)
		{
			A[i][j]=1;
		}
}

double Norm(double* X1, double* X2)
{
	double Result = 0.0;
	#pragma omp parallel for shared(Result)
	for (int I = 0; I < size; I++)
	{
		double ValueOfCurrentIteration = this->abs(X1[I] - X2[I]);
		if (ValueOfCurrentIteration > Result) Result = ValueOfCurrentIteration;
	}
	return Result;
}



void Equations::JacobiOpenMP()
{
	double startTime, endTime;
	startTime = MPI_Wtime();
	double* X = new double[size];
	double* X_last = new double[size];

	for (int I=0; I<size; I++)
		X_last[I] = B[I]/A[I*size + I];

	const double Epsilon = 0.0001;
	double norm;

	int IterationCounter = 1;
	do
	{
		if (IterationCounter > 1)
			this->X=X_last;

		#pragma omp parallel for
		for (int I = 0; I < size; I++)
		{
			X[I] = B[I];
			#pragma omp parallel for
			for (int J = 0; J < size; J++)
			{
				if (I != J)
					X[I] -= A[I*size + J] * X_last[J];
			}
			X[I] = X[I] / A[I*size + I];
			X[0];
			X[1];
		}
		norm = Norm(X, X_last);

		IterationCounter++;
	} while (norm > Epsilon);
	endTime = MPI_Wtime();
	time=(endTime-startTime);
	done_parallel = 1;
}
