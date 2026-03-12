#include <mpi.h>
using namespace std;

void MultVector(double k,double *X,double *Y,int N) {
	int world_size;
	MPI_Comm_size(MPI_COMM_WORLD, &world_size);
	int world_rank;
	MPI_Comm_rank(MPI_COMM_WORLD, &world_rank);
	MPI_Comm comm;
	double *Xbuffer;
	Xbuffer=new double[N/world_size];
	double *curr;
	curr=new double[N/world_size];
	MPI_Scatter(X,N/world_size,MPI_DOUBLE,Xbuffer,N/world_size,MPI_DOUBLE,0,MPI_COMM_WORLD);
	for (int i=0;i<N/world_size;i++) curr[i]=k*Xbuffer[i];
	MPI_Gather(curr,N/world_size,MPI_DOUBLE,Y,N/world_size,MPI_DOUBLE,0,MPI_COMM_WORLD);
	delete [] Xbuffer;
	delete [] curr;
	return;
}
