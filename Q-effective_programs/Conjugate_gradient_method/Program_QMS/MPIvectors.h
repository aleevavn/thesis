#include <mpi.h>
#include <iostream>
using namespace std;

double Multiply(double *X,double *Y,int N) {
	int world_size;
	MPI_Comm_size(MPI_COMM_WORLD, &world_size);
	int world_rank;
	MPI_Comm_rank(MPI_COMM_WORLD, &world_rank);
	MPI_Comm comm;
	double *Xbuffer;
	Xbuffer=new double[N/world_size];
	double *Ybuffer;
	Ybuffer=new double[N/world_size];
	double *curr;
	curr=new double[N/world_size];
	MPI_Scatter(X,N/world_size,MPI_DOUBLE,Xbuffer,N/world_size,MPI_DOUBLE,0,MPI_COMM_WORLD);
	MPI_Scatter(Y,N/world_size,MPI_DOUBLE,Ybuffer,N/world_size,MPI_DOUBLE,0,MPI_COMM_WORLD);
	double *prods;
	prods=new double[N/world_size];
	for (int i=0;i<N/world_size;i++) curr[i]=Xbuffer[i]*Ybuffer[i];
	MPI_Reduce(curr,prods,N/world_size,MPI_DOUBLE,MPI_SUM,0,MPI_COMM_WORLD);
	double Sum=0;
	for (int j=0;j<N/world_size;j++) Sum+=prods[j];
	delete [] Xbuffer;
	delete [] Ybuffer;
	delete [] curr;
	delete prods;
	return Sum;
}
