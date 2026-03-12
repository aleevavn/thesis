#include <nlohmann/json.hpp>
#include <iostream>
#include <fstream>
#include <map>
#include <set>
#include<string>
#include <sstream>
#include <cstdlib>
#include <filesystem>
#include <omp.h>
class QTerm
{
public:
	std::string op;
	std::unique_ptr<QTerm> fO;
	std::unique_ptr<QTerm> sO;
	QTerm* ancestor;
	std::unordered_map < std::string, unsigned long long> operationsDict;
	std::set<std::string> inputData;
	std::string string;
	QTerm(){}
	QTerm(std::string str, QTerm* ancestor = nullptr)
	{
		std::string fo,so;
		nlohmann::json jstr = nlohmann::json::parse(str, nullptr, false);
#pragma omp parallel sections
		{
#pragma omp section
			{
				fo = to_string(jstr["fO"]);
			}
#pragma omp section
			{
				so = to_string(jstr["sO"]);
			}
		}
#pragma omp parallel sections
		{
#pragma omp section
			{
				std::replace(fo.begin(), fo.end(), '[', '(');
				std::replace(fo.begin(), fo.end(), ']', ')');
			}
#pragma omp section
			{
				std::replace(so.begin(), so.end(), '[', '(');
				std::replace(so.begin(), so.end(), ']', ')');
			}
#pragma omp section
			{
				if (jstr["op"].is_string())
				{
					op = to_string(jstr["op"]);
					op.erase(std::remove(op.begin(), op.end(), '\"'), op.end());
				}
			}
		}
#pragma omp parallel sections
		{
#pragma omp section
			{
				if (jstr["fO"].is_object())
					fO = std::make_unique<QTerm>(fo, this);
				else
				{
					fo.erase(std::remove(fo.begin(), fo.end(), '\"'), fo.end());
					if (!isNumeric(fo))
						inputData.insert(fo);
				}
			}
#pragma omp section
			{
				if (jstr["sO"].is_object())
					sO = std::make_unique<QTerm>(so, this);
				else
				{
					this->ancestor = ancestor;
					so.erase(std::remove(so.begin(), so.end(), '\"'), so.end());
					if (!isNumeric(so))
						inputData.insert(so);
				}
			}
#pragma omp section
			{
				string = get_expression(jstr);
			}
		}
				if (ancestor != nullptr)
				{
#pragma omp parallel sections
					{
#pragma omp section
						{
#pragma omp parallel for
							for (std::unordered_map<std::string, unsigned long long>::iterator it = operationsDict.begin(); it != operationsDict.end(); ++it)
								ancestor->operationsDict[it->first]++;
						}
#pragma omp section
						{
#pragma omp parallel for
							for (std::set<std::string>::iterator it = inputData.begin(); it != inputData.end(); ++it)
								ancestor->inputData.insert(*it);
						}
					}
					if (string != "" && stringIsCorrect(string))
						ancestor->operationsDict[string]++;
				}
				else
				{
					if (string != "" && stringIsCorrect(string))
						operationsDict[string]++;
				}
			}
	private:
		
	static bool isNumeric(std::string const& str)
	{
		return !str.empty() && std::all_of(str.begin(), str.end(), ::isdigit);
	}
	static unsigned long long countOfSubstr(std::string str, std::string substr)
	{
		unsigned long long index = 0;
		unsigned long long count = 0;
		while ((index = str.find(substr, index)) != std::string::npos) {
			index += substr.length();
			count++;
		}
		return count;
	}
	static bool stringIsCorrect(std::string str)
	{
		unsigned long long count = 0;
		const std::vector<std::string> charList = { "&","<",">","!=","==" };
#pragma omp parallel for reduction(+:count)
		for (long long i = 0; i < charList.size(); i++)
		{
			count += countOfSubstr(str, charList[i]);
		}
		if (count == 1) return true;
		return false;
	}
	static std::string get_expression(nlohmann::json jstr)
	{
		if (!jstr.is_string())
			return "(" + get_expression(jstr["fO"]) + get_expression(jstr["op"]) + get_expression(jstr["sO"]) + ")";
			std::string text=to_string(jstr);
		text.erase(std::remove(text.begin(), text.end(), '\"'), text.end());
		return text;
	}
};
class QString
{

public:
	QTerm logical = QTerm();
	std::vector<std::string> values;
	std::map<std::string, unsigned long long> data;
	std::unordered_map<std::string, unsigned long long> operations;
	QString(std::string line, std::unordered_map<std::string, std::set<QString*>>& Operations, std::unordered_map<std::string, std::set<QString*>>& Data)
	{
		std::string element;
		std::stringstream ss(line.substr(line.find(';') + 1));
		std::string segment;
#pragma omp parallel sections
		{
#pragma omp section
			{
				std::string element=line.substr(0,line.find('='));
				data[element]++;
				std::string logical_part = line.substr(0, line.find(";", 0)).substr(line.find('=') + 1);
				logical_part.erase(std::remove(logical_part.begin(), logical_part.end(), ' '));
				if (logical_part != "")
					logical = QTerm(line.substr(0, line.find(";", 0)).substr(line.find('=') + 1));
			}
#pragma omp section
			{
				while (std::getline(ss, segment, ';'))
				{
					nlohmann::json x = nlohmann::json::parse(segment, nullptr, false);
					if (x.is_string() || x.is_discarded())
					{
						values.push_back(segment);
					}
					else if (x.is_number())
					{
						values.push_back(segment);
					}
					else if (x.is_object())
					{
						auto y = QTerm(segment);
						values.push_back(y.string);
					}
				}
			}
		}
#pragma omp parallel sections
		{
#pragma omp section
			{
#pragma omp parallel for
				for (auto it = logical.operationsDict.begin(); it != logical.operationsDict.end(); ++it)
					operations[it->first]++;
			}
		}
		fillTheLists(Operations, Data);
	}
private:
	void fillTheLists(std::unordered_map<std::string, std::set<QString*>>& Operations, std::unordered_map<std::string, std::set<QString*>>& Data)
	{
#pragma omp parallel sections
		{
#pragma omp section
			{
#pragma omp parallel for
				for (auto it = data.begin(); it != data.end(); ++it)
					Data[it->first].insert(this);
			}
#pragma omp section
			{
#pragma omp parallel for
				for (auto it = operations.begin(); it != operations.end(); ++it)
					Operations[it->first].insert(this);
			}
		}
	}
};
int main(int argc, char** argv)
{
	double start = omp_get_wtime();
	std::unordered_map<std::string, std::set<QString*>> operations;
	std::unordered_map<std::string, std::set<QString*>> data;
	std::unordered_map <std::string, std::string> files;
	std::set<QString*> listofQStrings;
	std::ifstream in(argv[1]);
	char dirName[255];
	if (in.is_open())
	{
		std::string line;
		unsigned long long dirNum = 0;
		unsigned long long fileNum = 0;
		while (std::getline(in, line))
		{
			auto it = QString(line, operations, data);
			std::error_code code;
			std::string dir_path;
			auto element = &it;
			for (auto& y : element->data)
				dir_path += y.first + ";";
			dir_path = dir_path.substr(0, dir_path.size() - 1);
			if (files.find(dir_path) == files.end())
			{
				sprintf_s(dirName, "%X", dirNum);
				files[dir_path] = dirName;
				dirNum++;
				std::filesystem::create_directory(dirName, code);
				if (code)
					for (const auto& entry : std::filesystem::directory_iterator(dir_path))
						std::filesystem::remove_all(entry.path(), code);
				std::filesystem::current_path(dirName);
			}
			else
			{
				std::filesystem::current_path(files[dir_path]);
			}
			char fileName[255];
#pragma omp single
			{
				sprintf_s(fileName, "%X", fileNum);
				fileNum++;
			}
			std::ofstream out(fileName);
			if (out.is_open())
			{
				bool isFirst = true;
				for (auto& y : element->values)
				{
					if (!isFirst)
						out << ";";
					out << y;
					isFirst = false;

				}
				out << ";" << std::endl;
				for (auto& y : element->operations)
				{
					out << y.first << std::endl;
				}

			}
			out.close();
			std::filesystem::current_path("..//");
		}

	}
	in.close();
	double stop = omp_get_wtime();
	std::ofstream out(argv[2]);
	{
		out << stop - start << std::endl;
	}
	out.close();
	return 0;

}
