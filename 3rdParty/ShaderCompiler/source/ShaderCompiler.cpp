#include "ShaderCompiler.h"

#include <glslang/Public/ShaderLang.h>
#include <SPIRV/GlslangToSpv.h>
#include <spirv-tools/optimizer.hpp>
#include <spirv_glsl.hpp>

#include <string>
#include <ostream>
#include <sstream>
#include <regex>
#include <unordered_map>
#include <unordered_set>
#include <cstdint>
#include <limits>
#include <stack>

const int TargetGlslVersion = 450;
const EProfile TargetGlslProfile = ENoProfile;

struct AttribInfo
{
	std::string name;
	ShaderVariableType type = SHADER_VARIABLE_TYPE_UNKNOWN;
	int location = -1;
};

struct UniformBlockInfo
{
	int binding = -1;
	int size = 0;
	ShaderStage stage = SHADER_STAGE_UNDEFINED;
};

struct UniformInfo
{
	std::string name;
	ShaderVariableType type = SHADER_VARIABLE_TYPE_UNKNOWN;
	int arraySize = 1;
	int blockIndex = -1;
	int blockOffset = 0;
	int binding = -1;
	int arrayStride = 0;
	int matrixStride = 0;
	ShaderStage stage = SHADER_STAGE_UNDEFINED;
};

struct ProgramReflection
{
	std::vector<AttribInfo> attribs;
	std::vector<UniformBlockInfo> uniformBlocks;
	std::vector<UniformInfo> uniforms;
};

class IOAllocator
{
public:
	explicit IOAllocator(int size)
	{
		m_freeRanges.push_back({ 0, size });
	}

	bool Allocate(int size, int* location)
	{
		for (auto it = m_freeRanges.begin(); it != m_freeRanges.end(); it++) {
			auto range = *it;
			if (size <= range.size) {
				it = m_freeRanges.erase(it);
				if (size < range.size)
					it = m_freeRanges.insert(it, { range.location + size, range.size - size });
				*location = range.location;
				return true;
			}
		}
		return false;
	}

	bool Reserve(int location, int size)
	{
		for (auto it = m_freeRanges.begin(); it != m_freeRanges.end(); it++) {
			auto range = *it;
			if (location >= range.location && size <= range.size) {
				it = m_freeRanges.erase(it);
				if (location + size < range.location + range.size)
					it = m_freeRanges.insert(it, { location + size, range.location + range.size - location - size });
				if (location > range.location)
					it = m_freeRanges.insert(it, { range.location, location - range.location });
				return true;
			}
		}
		return false;
	}

private:
	struct FreeRange
	{
		int location;
		int size;
	};

	std::vector<FreeRange> m_freeRanges;
};

const TBuiltInResource DefaultBuiltInResource = {
	/* .MaxLights = */ 32,
	/* .MaxClipPlanes = */ 6,
	/* .MaxTextureUnits = */ 32,
	/* .MaxTextureCoords = */ 32,
	/* .MaxVertexAttribs = */ 64,
	/* .MaxVertexUniformComponents = */ 4096,
	/* .MaxVaryingFloats = */ 64,
	/* .MaxVertexTextureImageUnits = */ 32,
	/* .MaxCombinedTextureImageUnits = */ 80,
	/* .MaxTextureImageUnits = */ 32,
	/* .MaxFragmentUniformComponents = */ 4096,
	/* .MaxDrawBuffers = */ 32,
	/* .MaxVertexUniformVectors = */ 128,
	/* .MaxVaryingVectors = */ 8,
	/* .MaxFragmentUniformVectors = */ 16,
	/* .MaxVertexOutputVectors = */ 16,
	/* .MaxFragmentInputVectors = */ 15,
	/* .MinProgramTexelOffset = */ -8,
	/* .MaxProgramTexelOffset = */ 7,
	/* .MaxClipDistances = */ 8,
	/* .MaxComputeWorkGroupCountX = */ 65535,
	/* .MaxComputeWorkGroupCountY = */ 65535,
	/* .MaxComputeWorkGroupCountZ = */ 65535,
	/* .MaxComputeWorkGroupSizeX = */ 1024,
	/* .MaxComputeWorkGroupSizeY = */ 1024,
	/* .MaxComputeWorkGroupSizeZ = */ 64,
	/* .MaxComputeUniformComponents = */ 1024,
	/* .MaxComputeTextureImageUnits = */ 16,
	/* .MaxComputeImageUniforms = */ 8,
	/* .MaxComputeAtomicCounters = */ 8,
	/* .MaxComputeAtomicCounterBuffers = */ 1,
	/* .MaxVaryingComponents = */ 60,
	/* .MaxVertexOutputComponents = */ 64,
	/* .MaxGeometryInputComponents = */ 64,
	/* .MaxGeometryOutputComponents = */ 128,
	/* .MaxFragmentInputComponents = */ 128,
	/* .MaxImageUnits = */ 8,
	/* .MaxCombinedImageUnitsAndFragmentOutputs = */ 8,
	/* .MaxCombinedShaderOutputResources = */ 8,
	/* .MaxImageSamples = */ 0,
	/* .MaxVertexImageUniforms = */ 0,
	/* .MaxTessControlImageUniforms = */ 0,
	/* .MaxTessEvaluationImageUniforms = */ 0,
	/* .MaxGeometryImageUniforms = */ 0,
	/* .MaxFragmentImageUniforms = */ 8,
	/* .MaxCombinedImageUniforms = */ 8,
	/* .MaxGeometryTextureImageUnits = */ 16,
	/* .MaxGeometryOutputVertices = */ 256,
	/* .MaxGeometryTotalOutputComponents = */ 1024,
	/* .MaxGeometryUniformComponents = */ 1024,
	/* .MaxGeometryVaryingComponents = */ 64,
	/* .MaxTessControlInputComponents = */ 128,
	/* .MaxTessControlOutputComponents = */ 128,
	/* .MaxTessControlTextureImageUnits = */ 16,
	/* .MaxTessControlUniformComponents = */ 1024,
	/* .MaxTessControlTotalOutputComponents = */ 4096,
	/* .MaxTessEvaluationInputComponents = */ 128,
	/* .MaxTessEvaluationOutputComponents = */ 128,
	/* .MaxTessEvaluationTextureImageUnits = */ 16,
	/* .MaxTessEvaluationUniformComponents = */ 1024,
	/* .MaxTessPatchComponents = */ 120,
	/* .MaxPatchVertices = */ 32,
	/* .MaxTessGenLevel = */ 64,
	/* .MaxViewports = */ 16,
	/* .MaxVertexAtomicCounters = */ 0,
	/* .MaxTessControlAtomicCounters = */ 0,
	/* .MaxTessEvaluationAtomicCounters = */ 0,
	/* .MaxGeometryAtomicCounters = */ 0,
	/* .MaxFragmentAtomicCounters = */ 8,
	/* .MaxCombinedAtomicCounters = */ 8,
	/* .MaxAtomicCounterBindings = */ 1,
	/* .MaxVertexAtomicCounterBuffers = */ 0,
	/* .MaxTessControlAtomicCounterBuffers = */ 0,
	/* .MaxTessEvaluationAtomicCounterBuffers = */ 0,
	/* .MaxGeometryAtomicCounterBuffers = */ 0,
	/* .MaxFragmentAtomicCounterBuffers = */ 1,
	/* .MaxCombinedAtomicCounterBuffers = */ 1,
	/* .MaxAtomicCounterBufferSize = */ 16384,
	/* .MaxTransformFeedbackBuffers = */ 4,
	/* .MaxTransformFeedbackInterleavedComponents = */ 64,
	/* .MaxCullDistances = */ 8,
	/* .MaxCombinedClipAndCullDistances = */ 8,
	/* .MaxSamples = */ 4,
	/* .maxMeshOutputVerticesNV = */ 256,
	/* .maxMeshOutputPrimitivesNV = */ 512,
	/* .maxMeshWorkGroupSizeX_NV = */ 32,
	/* .maxMeshWorkGroupSizeY_NV = */ 1,
	/* .maxMeshWorkGroupSizeZ_NV = */ 1,
	/* .maxTaskWorkGroupSizeX_NV = */ 32,
	/* .maxTaskWorkGroupSizeY_NV = */ 1,
	/* .maxTaskWorkGroupSizeZ_NV = */ 1,
	/* .maxMeshViewCountNV = */ 4,

	/* .limits = */ {
	/* .nonInductiveForLoops = */ 1,
	/* .whileLoops = */ 1,
	/* .doWhileLoops = */ 1,
	/* .generalUniformIndexing = */ 1,
	/* .generalAttributeMatrixVectorIndexing = */ 1,
	/* .generalVaryingIndexing = */ 1,
	/* .generalSamplerIndexing = */ 1,
	/* .generalVariableIndexing = */ 1,
	/* .generalConstantMatrixVectorIndexing = */ 1,
}
};

struct GlslangInitializer
{
	GlslangInitializer()
	{
		glslang::InitializeProcess();
	}

	~GlslangInitializer()
	{
		glslang::FinalizeProcess();
	}
};

struct UniformDecl
{
	uint32_t position;
	uint32_t length;
	std::string decl;
};

bool PreprocessLegacy(EShLanguage stage, const char* source, std::string* output, std::ostream& logger)
{
	glslang::TShader shader(stage);
	shader.setStrings(&source, 1);
	glslang::TShader::ForbidIncluder includer;
	auto status = shader.preprocess(&DefaultBuiltInResource, 100, ENoProfile, true, false, EShMsgOnlyPreprocessor, output, includer);
	logger << shader.getInfoLog();
	logger << shader.getInfoDebugLog();
	return status;
}

bool ValidateLegacy(EShLanguage stage, const char* source, std::ostream& logger)
{
	glslang::TShader shader(stage);
	shader.setStrings(&source, 1);
	auto success = shader.parse(&DefaultBuiltInResource, 100, ENoProfile, true, false, EShMsgDefault);
	logger << shader.getInfoLog();
	logger << shader.getInfoDebugLog();
	if (success) {
		glslang::TProgram program;
		program.addShader(&shader);
		success = program.link(EShMsgDefault);
		logger << program.getInfoLog();
		logger << program.getInfoDebugLog();
	}
	return success;
}

void StripVersion(std::string& source)
{
	std::regex regex("#version[ \\t]+\\d+([ \\t]+\\w+)?");
	source = std::regex_replace(source, regex, "");
}

bool IsOpaqueType(const std::string& name)
{
	static const std::unordered_set<std::string> opaqueTypes = { "sampler2D", "samplerCube" };
	return opaqueTypes.find(name) != opaqueTypes.end();
}

std::vector<UniformDecl> FindNonOpaqueUniformDecls(const std::string& source)
{
	std::vector<UniformDecl> decls;
	std::regex regex("uniform\\s+((\\w+\\s+)?(\\w+)\\s+\\w+(?:\\s*\\[.*?\\])?)\\s*;");
	std::sregex_iterator it(source.begin(), source.end(), regex);
	std::sregex_iterator end;
	for (; it != end; it++) {
		if (IsOpaqueType(it->str(3)))
			continue;
		UniformDecl decl;
		decl.position = it->position(0);
		decl.length = it->length(0);
		decl.decl = it->str(1);
		decls.push_back(std::move(decl));
	}
	return std::move(decls);
}

void GenerateUniformBlock(std::string& source, EShLanguage stage)
{
	auto uniformDecls = FindNonOpaqueUniformDecls(source);
	if (uniformDecls.empty())
		return;
	std::string blockDecl;
	if (stage == EShLangVertex)
		blockDecl += "uniform ShaderCompiler_VS_UniformBlock\n";
	else
		blockDecl += "uniform ShaderCompiler_FS_UniformBlock\n";
	blockDecl += "{\n";
	for (auto& decl : uniformDecls)
		blockDecl += "\t" + decl.decl + ";\n";
	blockDecl += "};\n";
	source.insert(uniformDecls.back().position + uniformDecls.back().length, blockDecl);
	for (auto decl = uniformDecls.rbegin(); decl != uniformDecls.rend(); decl++)
		source.erase(decl->position, decl->length);
}

void GeneratePreamble(std::string& source, EShLanguage stage)
{
	std::string preamble;
	preamble += "#version " + std::to_string(TargetGlslVersion) + "\n";
	preamble += "#define texture2D texture\n";
	preamble += "#define textureCube texture\n";
	if (stage == EShLangVertex) {
		preamble += "#define attribute in\n";
		preamble += "#define varying out\n";
	} else {
		preamble += "#define varying in\n";
		preamble += "#define gl_FragData _gl_FragData\n";
		preamble += "#define gl_FragColor gl_FragData[0]\n";
		preamble += "layout(location = 0) out mediump vec4 _gl_FragData[1];\n";
	}
	source.insert(0, preamble);
}

void ConvertToTarget(std::string& source, EShLanguage stage)
{
	StripVersion(source);
	GenerateUniformBlock(source, stage);
	GeneratePreamble(source, stage);
}

bool CompileTarget(EShLanguage stage, const char* source, std::vector<unsigned int>& spirv, std::ostream& logger)
{
	glslang::TShader shader(stage);
	shader.setStrings(&source, 1);
	shader.setEnvInput(glslang::EShSourceGlsl, stage, glslang::EShClientVulkan, 100);
	shader.setEnvClient(glslang::EShClientVulkan, glslang::EShTargetVulkan_1_0);
	shader.setEnvTarget(glslang::EShTargetSpv, glslang::EShTargetSpv_1_0);
	shader.setAutoMapBindings(true);
	shader.setAutoMapLocations(true);
	auto parsed = shader.parse(&DefaultBuiltInResource, TargetGlslVersion, TargetGlslProfile, false, false, EShMsgDefault);
	logger << shader.getInfoLog();
	logger << shader.getInfoDebugLog();
	if (!parsed) {
		return false;
	}
	glslang::TProgram program;
	program.addShader(&shader);
	auto linked = program.link(EShMsgDefault) && program.mapIO();
	logger << program.getInfoLog();
	logger << program.getInfoDebugLog();
	if (!linked) {
		return false;
	}
	glslang::GlslangToSpv(*program.getIntermediate(stage), spirv);
	spvtools::Optimizer optimizer(SPV_ENV_VULKAN_1_0);
	optimizer.RegisterPerformancePasses();
	std::vector<unsigned int> optimizedSpirv;
	optimizer.Run(spirv.data(), spirv.size(), &optimizedSpirv);
	spirv = std::move(optimizedSpirv);
	return true;
}

bool Compile(ShaderStage stage, const char* source, std::vector<unsigned int>& spirv, std::ostream& logger)
{
	GlslangInitializer glslangInitializer;
	auto glslangStage = stage == SHADER_STAGE_VERTEX ? EShLangVertex : EShLangFragment;
	std::string output;
	if (!PreprocessLegacy(glslangStage, source, &output, logger))
		return false;
	if (!ValidateLegacy(glslangStage, source, logger))
		return false;
	ConvertToTarget(output, glslangStage);
	if (!CompileTarget(glslangStage, output.c_str(), spirv, logger)) {
		return false;
	}
	return true;
}

ShaderVariableType ConvertShaderVariableType(const spirv_cross::Compiler& reflector, const spirv_cross::SPIRType& type)
{
	switch (type.basetype) {
		case spirv_cross::SPIRType::Boolean:
			if (type.columns == 1 && type.vecsize == 1)
				return SHADER_VARIABLE_TYPE_BOOL;
			if (type.columns == 1 && type.vecsize == 2)
				return SHADER_VARIABLE_TYPE_BOOL_VECTOR2;
			if (type.columns == 1 && type.vecsize == 3)
				return SHADER_VARIABLE_TYPE_BOOL_VECTOR3;
			if (type.columns == 1 && type.vecsize == 4)
				return SHADER_VARIABLE_TYPE_BOOL_VECTOR4;
			break;
		case spirv_cross::SPIRType::Int:
			if (type.columns == 1 && type.vecsize == 1)
				return SHADER_VARIABLE_TYPE_INT;
			if (type.columns == 1 && type.vecsize == 2)
				return SHADER_VARIABLE_TYPE_INT_VECTOR2;
			if (type.columns == 1 && type.vecsize == 3)
				return SHADER_VARIABLE_TYPE_INT_VECTOR3;
			if (type.columns == 1 && type.vecsize == 4)
				return SHADER_VARIABLE_TYPE_INT_VECTOR4;
			break;
		case spirv_cross::SPIRType::Float:
			if (type.columns == 1 && type.vecsize == 1)
				return SHADER_VARIABLE_TYPE_FLOAT;
			if (type.columns == 1 && type.vecsize == 2)
				return SHADER_VARIABLE_TYPE_FLOAT_VECTOR2;
			if (type.columns == 1 && type.vecsize == 3)
				return SHADER_VARIABLE_TYPE_FLOAT_VECTOR3;
			if (type.columns == 1 && type.vecsize == 4)
				return SHADER_VARIABLE_TYPE_FLOAT_VECTOR4;
			if (type.columns == 2 && type.vecsize == 2)
				return SHADER_VARIABLE_TYPE_FLOAT_MATRIX2;
			if (type.columns == 3 && type.vecsize == 3)
				return SHADER_VARIABLE_TYPE_FLOAT_MATRIX3;
			if (type.columns == 4 && type.vecsize == 4)
				return SHADER_VARIABLE_TYPE_FLOAT_MATRIX4;
			break;
		case spirv_cross::SPIRType::SampledImage:
			if (reflector.get_type(type.image.type).basetype == spirv_cross::SPIRType::Float) {
				if (type.image.dim == spv::Dim2D)
					return SHADER_VARIABLE_TYPE_SAMPLER_2D;
				if (type.image.dim == spv::Dim3D)
					return SHADER_VARIABLE_TYPE_SAMPLER_CUBE;
			}
			break;
	}
	return SHADER_VARIABLE_TYPE_UNKNOWN;
}

void ReflectBlockUniforms(
	const spirv_cross::Compiler& reflector, int blockIndex, uint32_t typeId, uint32_t offset,
	std::string path, ShaderStage stage, std::vector<UniformInfo>& uniforms)
{
	auto& type = reflector.get_type(typeId);
	for (uint32_t memberIndex = 0; memberIndex < type.member_types.size(); memberIndex++) {
		auto& memberType = reflector.get_type(type.member_types[memberIndex]);
		auto memberOffset = offset + reflector.type_struct_member_offset(type, memberIndex);
		auto memberPath = reflector.get_member_name(typeId, memberIndex);
		if (path.length() > 0)
			memberPath = path + "." + memberPath;
		if (memberType.basetype == spirv_cross::SPIRType::Struct && memberType.array.size() > 0) {
			auto arrayStride = reflector.type_struct_member_array_stride(type, memberIndex);
			for (uint32_t index = 0; index < memberType.array.back(); index++) {
				ReflectBlockUniforms(reflector, blockIndex, memberType.self, memberOffset + index * arrayStride,
					memberPath + "[" + std::to_string(index) + "]", stage, uniforms);
			}
		} else {
			UniformInfo info;
			info.stage = stage;
			info.name = memberPath;
			info.type = ConvertShaderVariableType(reflector, memberType);
			info.binding = -1;
			info.blockIndex = blockIndex;
			info.blockOffset = memberOffset;
			info.arraySize = 1;
			if (memberType.array.size() > 0) {
				info.arraySize = memberType.array.back();
				info.arrayStride = reflector.type_struct_member_array_stride(type, memberIndex);
				info.name += "[0]";
			}
			if (type.columns > 1)
				info.matrixStride = reflector.type_struct_member_matrix_stride(type, memberIndex);
			uniforms.push_back(std::move(info));
		}
	}
}

void ReflectUniformBlock(
	const spirv_cross::Compiler& reflector, const spirv_cross::Resource& resource,
	ShaderStage stage, ProgramReflection& reflection)
{
	auto& type = reflector.get_type(resource.base_type_id);
	ReflectBlockUniforms(reflector, reflection.uniformBlocks.size(), resource.base_type_id, 0, "", stage, reflection.uniforms);
	UniformBlockInfo info;
	info.stage = stage;
	info.binding = reflector.get_decoration(resource.id, spv::DecorationBinding);
	info.size = reflector.get_declared_struct_size(type);
	reflection.uniformBlocks.push_back(std::move(info));
}

void ReflectSamplers(
	const spirv_cross::Compiler& reflector, const spirv_cross::SmallVector<spirv_cross::Resource>& resources,
	ShaderStage stage, ProgramReflection& reflection)
{
	for (auto& sampler : resources) {
		auto& type = reflector.get_type(sampler.base_type_id);
		UniformInfo info;
		info.stage = stage;
		info.name = sampler.name;
		info.binding = reflector.get_decoration(sampler.id, spv::DecorationBinding);
		info.type = ConvertShaderVariableType(reflector, type);
		if (type.array.size() > 0)
			info.arraySize = type.array.back();
		reflection.uniforms.push_back(std::move(info));
	}
}

void ReflectAttribs(const spirv_cross::Compiler& reflector, const spirv_cross::SmallVector<spirv_cross::Resource>& resources, ProgramReflection& reflection)
{
	for (auto& attrib : resources) {
		auto& type = reflector.get_type(attrib.base_type_id);
		AttribInfo info;
		info.name = attrib.name;
		info.location = reflector.get_decoration(attrib.id, spv::DecorationLocation);
		info.type = ConvertShaderVariableType(reflector, type);
		reflection.attribs.push_back(std::move(info));
	}
}

void Reflect(const spirv_cross::Compiler& reflector, ProgramReflection& reflection)
{
	auto stage = reflector.get_execution_model() == spv::ExecutionModelVertex
		? SHADER_STAGE_VERTEX
		: SHADER_STAGE_FRAGMENT;
	auto activeInterfaceVariables = reflector.get_active_interface_variables();
	auto resources = reflector.get_shader_resources(activeInterfaceVariables);
	ReflectSamplers(reflector, resources.sampled_images, stage, reflection);
	if (resources.uniform_buffers.size() > 0)
		ReflectUniformBlock(reflector, resources.uniform_buffers.front(), stage, reflection);
	if (reflector.get_execution_model() == spv::ExecutionModelVertex)
		ReflectAttribs(reflector, resources.stage_inputs, reflection);
}

int CalculateTypeLocationSize(const spirv_cross::Compiler& reflector, uint32_t typeId)
{
	auto& type = reflector.get_type(typeId);
	if (type.pointer) {
		return CalculateTypeLocationSize(reflector, type.parent_type);
	}
	if (type.array.size() > 0) {
		return type.array.back() * CalculateTypeLocationSize(reflector, type.parent_type);
	}
	if (type.basetype == spirv_cross::SPIRType::Struct) {
		int size = 0;
		for (auto memberTypeId : type.member_types) {
			size += CalculateTypeLocationSize(reflector, memberTypeId);
		}
		return size;
	}
	int underlyingTypeSize;
	switch (type.basetype) {
		case spirv_cross::SPIRType::Boolean:
		case spirv_cross::SPIRType::Int:
		case spirv_cross::SPIRType::UInt:
		case spirv_cross::SPIRType::Float:
			underlyingTypeSize = 4;
			break;
		case spirv_cross::SPIRType::Double:
		case spirv_cross::SPIRType::Int64:
		case spirv_cross::SPIRType::UInt64:
			underlyingTypeSize = 8;
			break;
		default:
			underlyingTypeSize = 0;
			break;
	}
	return ((underlyingTypeSize * type.vecsize + 15) / 16) * type.columns;
}

void PatchDecorations(
	std::vector<unsigned int>& spirv,
	spv::Decoration decoration,
	const spirv_cross::Compiler& reflector,
    const spirv_cross::SmallVector<spirv_cross::Resource>& resources)
{
	for (auto& resource : resources) {
		uint32_t wordOffset;
		reflector.get_binary_offset_for_decoration(resource.id, decoration, wordOffset);
		spirv[wordOffset] = reflector.get_decoration(resource.id, decoration);
	}
}

bool Link(
	std::vector<unsigned int>& vsSpirv,
	std::vector<unsigned int>& fsSpirv,
	const std::unordered_map<std::string, int>& attribLocations,
	ProgramReflection& reflection,
	std::ostream& logger)
{
	spirv_cross::CompilerGLSL vsReflector(vsSpirv);
	spirv_cross::CompilerGLSL fsReflector(fsSpirv);
	auto vsResources = vsReflector.get_shader_resources();
	auto fsResources = fsReflector.get_shader_resources();
	IOAllocator attribAllocator(std::numeric_limits<int>::max());
	IOAllocator varyingAllocator(std::numeric_limits<int>::max());
	std::unordered_map<std::string, int> varyingLocations;
	for (auto& attrib : vsResources.stage_inputs) {
		auto locationIt = attribLocations.find(attrib.name);
		if (locationIt != attribLocations.end()) {
			attribAllocator.Reserve(locationIt->second, CalculateTypeLocationSize(vsReflector, attrib.type_id));
		}
	}
	for (auto& attrib : vsResources.stage_inputs) {
		int location;
		auto locationIt = attribLocations.find(attrib.name);
		if (locationIt != attribLocations.end()) {
			location = locationIt->second;
		} else {
			attribAllocator.Allocate(CalculateTypeLocationSize(vsReflector, attrib.type_id), &location);
		}
		vsReflector.set_decoration(attrib.id, spv::DecorationLocation, location);
	}
	for (auto& varying : vsResources.stage_outputs) {
		int location;
		varyingAllocator.Allocate(CalculateTypeLocationSize(vsReflector, varying.type_id), &location);
		varyingLocations[varying.name] = location;
		vsReflector.set_decoration(varying.id, spv::DecorationLocation, location);
	}
	for (auto& varying : fsResources.stage_inputs) {
		auto locationIt = varyingLocations.find(varying.name);
		if (locationIt == varyingLocations.end()) {
			logger << "Could not resolve fragment shader varying location " << varying.name << std::endl;
			return false;
		}
		fsReflector.set_decoration(varying.id, spv::DecorationLocation, locationIt->second);
	}
	int bindingCount = 0;
	for (auto& resource : vsResources.sampled_images) {
		vsReflector.set_decoration(resource.id, spv::DecorationBinding, bindingCount++);
	}
	for (auto& resource : vsResources.uniform_buffers) {
		vsReflector.set_decoration(resource.id, spv::DecorationBinding, bindingCount++);
	}
	for (auto& resource : fsResources.sampled_images) {
		fsReflector.set_decoration(resource.id, spv::DecorationBinding, bindingCount++);
	}
	for (auto& resource : fsResources.uniform_buffers) {
		fsReflector.set_decoration(resource.id, spv::DecorationBinding, bindingCount++);
	}
	PatchDecorations(vsSpirv, spv::DecorationLocation, vsReflector, vsResources.stage_inputs);
	PatchDecorations(vsSpirv, spv::DecorationLocation, vsReflector, vsResources.stage_outputs);
	PatchDecorations(vsSpirv, spv::DecorationBinding, vsReflector, vsResources.sampled_images);
	PatchDecorations(vsSpirv, spv::DecorationBinding, vsReflector, vsResources.uniform_buffers);
	PatchDecorations(fsSpirv, spv::DecorationLocation, fsReflector, fsResources.stage_inputs);
	PatchDecorations(fsSpirv, spv::DecorationBinding, fsReflector, fsResources.sampled_images);
	PatchDecorations(fsSpirv, spv::DecorationBinding, fsReflector, fsResources.uniform_buffers);
	ProgramReflection programReflection;
	Reflect(vsReflector, programReflection);
	Reflect(fsReflector, programReflection);
	reflection = std::move(programReflection);
	return true;
}

struct Shader
{
	std::vector<unsigned int> spirv;
	std::string infoLog;
};

ShaderHandle CreateShader()
{
	return new Shader();
}

int CompileShader(ShaderHandle shaderHandle, ShaderStage stage, const char* source)
{
	auto shader = static_cast<Shader*>(shaderHandle);
	std::ostringstream logger;
	std::vector<unsigned int> spirv;
	auto status = Compile(stage, source, spirv, logger);
	shader->spirv = std::move(spirv);
	shader->infoLog = logger.str();
	return status;
}

const char* GetShaderInfoLog(ShaderHandle shaderHandle)
{
	return static_cast<Shader*>(shaderHandle)->infoLog.c_str();
}

unsigned int GetShaderSpvSize(ShaderHandle shaderHandle)
{
	return static_cast<Shader*>(shaderHandle)->spirv.size() * sizeof(unsigned int);
}

const void* GetShaderSpv(ShaderHandle shaderHandle)
{
	return static_cast<Shader*>(shaderHandle)->spirv.data();
}

void SetShaderSpv(ShaderHandle shaderHandle, const void* spv, unsigned int size)
{
	static_cast<Shader*>(shaderHandle)->spirv.assign((unsigned int*)spv, (unsigned int*)((char*)spv + size));
}

void DestroyShader(ShaderHandle shaderHandle)
{
	delete static_cast<Shader*>(shaderHandle);
}

struct Program
{
	std::unordered_map<std::string, int> attribLocations;
	std::vector<unsigned int> vsSpirv;
	std::vector<unsigned int> fsSpirv;
	ProgramReflection reflection;
	std::string infoLog;
};

ProgramHandle CreateProgram()
{
	return new Program();
}

void BindAttribLocation(ProgramHandle programHandle, const char* name, int location)
{
	static_cast<Program*>(programHandle)->attribLocations[name] = location;
}

int LinkProgram(ProgramHandle programHandle, ShaderHandle vsHandle, ShaderHandle fsHandle)
{
	auto program = static_cast<Program*>(programHandle);
	auto vs = static_cast<Shader*>(vsHandle);
	auto fs = static_cast<Shader*>(fsHandle);
	std::ostringstream logger;
	program->vsSpirv = vs->spirv;
	program->fsSpirv = fs->spirv;
	auto status = Link(program->vsSpirv, program->fsSpirv, program->attribLocations, program->reflection, logger);
	program->infoLog = logger.str();
	return status;
}

const char* GetProgramInfoLog(ProgramHandle programHandle)
{
	return static_cast<Program*>(programHandle)->infoLog.c_str();
}

unsigned int GetSpvSize(ProgramHandle programHandle, ShaderStage stage)
{
	auto program = static_cast<Program*>(programHandle);
	switch (stage) {
		case SHADER_STAGE_VERTEX:
			return program->vsSpirv.size() * sizeof(unsigned int);
		case SHADER_STAGE_FRAGMENT:
			return program->fsSpirv.size() * sizeof(unsigned int);
		default:
			return 0;
	}
}

const void* GetSpv(ProgramHandle programHandle, ShaderStage stage)
{
	auto program = static_cast<Program*>(programHandle);
	switch (stage) {
		case SHADER_STAGE_VERTEX:
			return program->vsSpirv.data();
		case SHADER_STAGE_FRAGMENT:
			return program->fsSpirv.data();
		default:
			return 0;
	}
}

int GetActiveAttribCount(ProgramHandle programHandle)
{
	return static_cast<Program*>(programHandle)->reflection.attribs.size();
}

const char* GetActiveAttribName(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.attribs[index].name.c_str();
}

ShaderVariableType GetActiveAttribType(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.attribs[index].type;
}

int GetActiveAttribLocation(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.attribs[index].location;
}

int GetActiveUniformBlockCount(ProgramHandle programHandle)
{
	return static_cast<Program*>(programHandle)->reflection.uniformBlocks.size();
}

int GetActiveUniformBlockBinding(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniformBlocks[index].binding;
}

int GetActiveUniformBlockSize(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniformBlocks[index].size;
}

ShaderStage GetActiveUniformBlockStage(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniformBlocks[index].stage;
}

int GetActiveUniformCount(ProgramHandle programHandle)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms.size();
}

const char* GetActiveUniformName(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].name.c_str();
}

ShaderVariableType GetActiveUniformType(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].type;
}

int GetActiveUniformArraySize(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].arraySize;
}

int GetActiveUniformArrayStride(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].arrayStride;
}

int GetActiveUniformMatrixStride(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].matrixStride;
}

ShaderStage GetActiveUniformStage(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].stage;
}

int GetActiveUniformBinding(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].binding;
}

int GetActiveUniformBlockIndex(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].blockIndex;
}

int GetActiveUniformBlockOffset(ProgramHandle programHandle, int index)
{
	return static_cast<Program*>(programHandle)->reflection.uniforms[index].blockOffset;
}

void DestroyProgram(ProgramHandle programHandle)
{
	delete static_cast<Program*>(programHandle);
}
