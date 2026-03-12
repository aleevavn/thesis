#include <mpi.h>
#include <iostream>
#include <fstream>
#include <cstdlib>
#include "MPIvectors.h"
#include "MPIsumvectors.h"
#include "MPIprod.h"
using namespace std;

double dabs(double x) {
	return x>=0?x:-x;
}

bool isSignificant(double x,double threshold) {
	return dabs(x)>=threshold?true:false;
}

void conjgrads(int n,double **a,double *b,double *x0,double *x,double maxaccerr) {
	int count=0;
// Объявление нужных векторов и чисел
// Текущее приближение
	double *ongoing;
	ongoing=new double[n];
	#pragma omp parallel for private(i) {
	for (int i=0;i<n;i++) ongoing[i]=x0[i];
	#pragma omp parallel }
// Невязка
	double *r;
	r=new double[n];
	double *m;
	m=new double[n];
	double *pre_r;
	pre_r=new double[n];
	#pragma omp parallel for private(i) {
	for (int i=0;i<n;i++) pre_r[i]=-Multiply(a[i],x0,n);
	#pragma omp parallel }
	SumVectors(b,pre_r,r,n);
// Направление
	double *direction;
	direction=new double[n];
	#pragma omp parallel for private(i) shared(n,r,direction) {
	for (int i=0;i<n;i++) direction[i]=r[i];
	#pragma omp parallel }
	double error=100;
// Вспомогательные переменные
	double A,a1,a2,B,b2;
	double b1=0;
	bool checkpoint=false;
	double *a3;
	a3=new double[n];
	double *oldr;
	oldr=new double[n];
	double *oldp;
	oldp=new double[n];
	double *previous;
	previous=new double[n];
	double **transa;
	transa=new double*[n];
	for (int i=0;i<n;i++) transa[i]=new double[n];
	for (int i=0;i<n;i++) for (int j=0;j<n;j++) transa[i][j]=a[j][i];
// Поиск решения
	while (error>=maxaccerr) {
		count++;
// Вычисление коэффициента A
		#pragma omp parallel for private(i) {
		for (int i=0;i<n;i++) a3[i]=0;
		for (int i=0;i<n;i++) oldr[i]=r[i];
		for (int i=0;i<n;i++) oldp[i]=direction[i];
		for (int i=0;i<n;i++) previous[i]=ongoing[i];
		#pragma omp parallel }
		a1=0;
		a2=0;
		b2=0;
		#pragma omp parallel for private(i,j) {
		a1=checkpoint?b1:Multiply(r,r,n);
		for (int i=0;i<n;i++) a3[i]=Multiply(direction,transa[i],n);
		a2=Multiply(a3,direction,n);
		#pragma omp parallel }
		A=a1/a2;
// Вычисление нового приближения
		#pragma omp parallel for private(i,j) shared(n,ongoing,A,direction,a) {
		for (int i=0;i<n;i++) ongoing[i]+=A*direction[i];
// Вычисление новой невязки
		for (int i=0;i<n;i++) r[i]-=A*Multiply(a[i],direction,n);
		#pragma omp parallel }
// Вычисление значения B
		#pragma omp parallel for private(i,j) shared(n,b1,b2,r,oldr) {
		b1=Multiply(r,r,n);
		checkpoint=true;
		b2=Multiply(oldr,oldr,n);
		#pragma omp parallel }
		B=b1/b2;
// Вычисление нового направления
		#pragma omp parallel for private(i) {
		MultVector(B,direction,m,n);
		SumVectors(r,m,direction,n);
		#pragma omp parallel }
// Вычисление погрешности
		#pragma omp parallel for private(i) {
		error=Multiply(r,r,n)/Multiply(b,b,n);
		#pragma omp parallel }
	}
// Освобождение памяти и возвращение результата
	for (int i=0;i<n;i++) x[i]=ongoing[i];
	delete [] ongoing;
	delete [] r;
	delete [] direction;
	delete [] a3;
	delete [] oldr;
	delete [] oldp;
	delete [] previous;
	return;
}

int main(int argc,char *argv[]) {
	if (argc==1) {
		printf("No file given.\n");
		return 2;
	}
	MPI_Init(NULL, NULL);
	int n;
	char readthis[100];
// Чтение файла
	ifstream source(argv[1]);
	source>>readthis;
	n=atoi(readthis);
	double **leftpart;
	leftpart=new double*[n];
	for (int i=0;i<n;i++) leftpart[i]=new double[n];
	double *rightpart;
	rightpart=new double[n];
	double *start;
	start=new double[n];
	double *solution;
	solution=new double[n];
	for (int i=0;i<n;i++) {
		for (int j=0;j<n;j++) {
			source>>readthis;
			leftpart[i][j]=atof(readthis);
		}			
		source>>readthis;
		rightpart[i]=atof(readthis);
	}
	for (int i=0;i<n;i++) {
		source>>readthis;
		start[i]=atof(readthis);
	}
	source>>readthis;
	double err=atof(readthis);
	source.close();
// Решение уравнения
	conjgrads(n,leftpart,rightpart,start,solution,err);
	delete [] leftpart;
	for (int i=0;i<n;i++) cout <<solution[i]<<"\n";
	delete [] rightpart;
	delete [] solution;
	delete [] start;
	MPI_Finalize();
	return 0;
}
