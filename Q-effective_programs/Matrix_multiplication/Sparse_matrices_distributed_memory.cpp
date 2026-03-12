MPI_Bcast(A, Size , MPI_DOUBLE, 0, MPI_COMM_WORLD);
MPI_Bcast(B, Size , MPI_DOUBLE, 0, MPI_COMM_WORLD);

int lower_bound = rank * ( Size / num_worker);
int upper_bound = ((rank + 1) * ( Size / num_worker))-1;
    
#pragma omp parallel for private (i, j, k)
for (int i = lower_bound; i < upper_bound; i++)
{
    int lower_bound_A = A[2][i];
    int upper_bound_A = A[2][i+1];
    int lower_bound_B = B[2][i];
    int upper_bound_B = B[2][i+1];
    if ((upper_bound_A-lower_bound_A != 0) & (upper_bound_B-
                                                       lower_bound_B !=0)) 
        for (int j = lower_bound_A; j < upper_bound_A; j++)
            for (int k = lower_bound_B; k < upper_bound_B; k++)
                if (A[1][lower_bound_A+j] == B[1][lower_bound_B+k])
                    M[i][j].push_back(A[0][lower_bound_A+j]*
                                                   B[0][lower_bound_B+k]);
}

for (int g = 1; g < Size; g *= 2) 
{
#pragma omp parallel for private (i, j, k, cell)
    for (int i = 0; i < Size; i++)
        for (int j = 0; j < Size; j++)
            for (int k = 0; k < M[i][j].size(); k++) 
            {
                int cell = g * k;
                M[i][j][cell] += M[i][j][cell + g];
                M[i][j][cell + g] = 0;
            }
}

for (int i = 0; i < Size; i++)
{
    for (int j = 0;  j < Size; j++)
        if (M[i][j] != 0) {
            C[1].push_back(j);
            C[0].push_back(M[i][j]);
        }
        C[2].push_back(Size);
}
    
       MPI_Gather(C, Size, MPI_DOUBLE,
           result_matrix, Size,  MPI_DOUBLE, 0, MPI_COMM_WORLD);

