#include "stdafx.h"
#include <iostream>
#include <fstream>
#include <ctime>


using namespace std;

int main(int argc, char* argv[])
{
	srand(time(0));

	ifstream file1("matrix.txt"), file2("array.txt");
	int N, M = 0, L = 0, S = 0, K = 0;

	file1 >> N;

	double **ArrA = new double *[N];
	double **ArrB = new double *[N];
	double *F = new double[N];
	double *F1 = new double[N];
	double *Ans = new double[N];
	double *Alph = new double[N + 1];
	double *Bet = new double[N + 1];

	for (int i = 0; i < N; i++)
	{
		ArrA[i] = new double[N];
		ArrB[i] = new double[N];

		for (int j = 0; j < N; j++)
		{
			file1 >> ArrA[i][j];
		}

		file2 >> F[i];
		file2 >> F1[i];
	}

	double C = ArrA[L][M];
	double A = -ArrA[L + 1][M];
	int ii = 2;
	

	for (int j = 0; j < N - 1; j++) 
       {
		ArrB[ii - 1][j] = ArrA[ii - 1][j] / C * A;
		L++;
		M++;
		ii++;
	}


	for (int j = 0; j < N - 1; j++) 
       {
		F1[ii - 1] = F1[ii - 1] + F1[ii - 2] / C * A;
		L++;
		M++;
		ii++;
	}

	for (int i = 1; i < N; i++) 
       {
		

		ArrB[i - 1][N - 1] = ArrA[i - 1][N - 1];

		
		for (int j = 0; j < N; j++) 
       {
			ArrA[i][j] = ArrA[i][j] + ArrB[i - 1][j];
		}
	}

	double C1 = ArrB[S][K];
	double A1 = ArrB[S + 1][K];
	double B1 = ArrB[S][K + 1];
	double F0 = Ans[S];

	Alph[0] = B1 / C1;
	Bet[0] = F0 / C1;


	for (int j = 1; j < N + 1; j++) 
       {
		S++;
		K++;
		Alph[j] = B1 / (C1 - A1 * Alph[j - 1]);
		Bet[j] = (F0 + A1 * Bet[j - 1]) / (C1 - A1 * Alph[j - 1]);
	}

	Ans[N - 1] = Bet[N];

	for (int j = N - 2; j > 0; j--) 
       {
		Ans[j] = Alph[j + 1] * Ans[j + 1] + Bet[j + 1];
	}



	ofstream file3("output.txt");

	for (int i = 0; i < N; i++) 
       {
		file3 << Ans[i] << " ";
	}


	file3 << "runtime = " << clock() / 1000.0 << endl; // גנולÿ נאבמעû ןנמדנאללû                  
	system("pause");


    return 0;
}


