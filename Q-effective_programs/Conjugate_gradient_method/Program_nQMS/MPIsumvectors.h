#include <mpi.h>
using namespace std;

void SumVectors(double *X,double *Y,double *Z,int N) {
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
	for (int i=0;i<N/world_size;i++) curr[i]=Xbuffer[i]+Ybuffer[i];
	MPI_Gather(curr,N/world_size,MPI_DOUBLE,Z,N/world_size,MPI_DOUBLE,0,MPI_COMM_WORLD);
	delete [] Xbuffer;
	delete [] Ybuffer;
	delete [] curr;
	return;
}
