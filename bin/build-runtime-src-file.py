#!/usr/bin/env python

################################################################################

src_ArrayObjs                     = 'core/ArrayObjs.cs'
src_BinRelIter                    = 'core/BinRelIter.cs'
src_BlankObj                      = 'core/BlankObj.cs'
src_Builder                       = 'core/Builder.cs'
src_EmptyRelObj                   = 'core/EmptyRelObj.cs'
src_EmptySeqObj                   = 'core/EmptySeqObj.cs'
src_FloatArrayObjs                = 'core/FloatArrayObjs.cs'
src_FloatObj                      = 'core/FloatObj.cs'
src_IntArrayObjs                  = 'core/IntArrayObjs.cs'
src_IntObj                        = 'core/IntObj.cs'
src_NeBinRelObj                   = 'core/NeBinRelObj.cs'
src_NeFloatSeqObj                 = 'core/NeFloatSeqObj.cs'
src_NeIntSeqObj                   = 'core/NeIntSeqObj.cs'
src_NeSeqObj                      = 'core/NeSeqObj.cs'
src_NeSetObj                      = 'core/NeSetObj.cs'
src_NeTernRelObj                  = 'core/NeTernRelObj.cs'
src_NeTreeMapObj                  = 'core/NeTreeMapObj.cs'
src_NeTreeSetObj                  = 'core/NeTreeSetObj.cs'
src_NullObj                       = 'core/NullObj.cs'
src_Obj                           = 'core/Obj.cs'
src_OptTagRecObj                  = 'core/OptTagRecObj.cs'
src_Procs                         = 'core/Procs.cs'
src_RecordObj                     = 'core/RecordObj.cs'
src_SeqIter                       = 'core/SeqIter.cs'
src_SeqObj                        = 'core/SeqObj.cs'
src_SetIter                       = 'core/SetIter.cs'
src_SymbObj                       = 'core/SymbObj.cs'
src_TaggedIntObj                  = 'core/TaggedIntObj.cs'
src_TaggedObj                     = 'core/TaggedObj.cs'
src_TernRelIter                   = 'core/TernRelIter.cs'

src_AbstractULongSorter           = 'utils/AbstractULongSorter.cs'
src_Algs                          = 'utils/Algs.cs'
src_Array                         = 'utils/Array.cs'
src_Canonical                     = 'utils/Canonical.cs'
src_CharStream                    = 'utils/CharStream.cs'
src_Conversions                   = 'utils/Conversions.cs'
src_DateTimeUtils                 = 'utils/DateTimeUtils.cs'
src_Debugging                     = 'utils/Debugging.cs'
src_ErrorHandling                 = 'utils/ErrorHandling.cs'
src_Hashing                       = 'utils/Hashing.cs'
src_IO                            = 'utils/IO.cs'
src_Miscellanea                   = 'utils/Miscellanea.cs'
src_ObjPrinter                    = 'utils/ObjPrinter.cs'
src_ObjVisitor                    = 'utils/ObjVisitor.cs'
src_Parser                        = 'utils/Parser.cs'
src_SymbTableFastCache            = 'utils/SymbTableFastCache.cs'
src_Tokenizer                     = 'utils/Tokenizer.cs'

src_ArraySliceAllocator           = 'automata/ArraySliceAllocator.cs'
src_BinaryTable                   = 'automata/BinaryTable.cs'
src_BinaryTableUpdater            = 'automata/BinaryTableUpdater.cs'
src_ColumnBase                    = 'automata/ColumnBase.cs'
src_FloatColumn                   = 'automata/FloatColumn.cs'
src_FloatColumnUpdater            = 'automata/FloatColumnUpdater.cs'
src_Index                         = 'automata/Index.cs'
src_IntColumn                     = 'automata/IntColumn.cs'
src_IntColumnUpdater              = 'automata/IntColumnUpdater.cs'
src_IntStore                      = 'automata/IntStore.cs'
src_IntStoreUpdater               = 'automata/IntStoreUpdater.cs'
src_KeyViolationException         = 'automata/KeyViolationException.cs'
src_ObjColumn                     = 'automata/ObjColumn.cs'
src_ObjColumnUpdater              = 'automata/ObjColumnUpdater.cs'
src_ObjStore                      = 'automata/ObjStore.cs'
src_ObjStoreUpdater               = 'automata/ObjStoreUpdater.cs'
src_OneWayBinTable                = 'automata/OneWayBinTable.cs'
src_OverflowTable                 = 'automata/OverflowTable.cs'
src_RelAuto                       = 'automata/RelAuto.cs'
src_Sym12TernaryTable             = 'automata/Sym12TernaryTable.cs'
src_Sym12TernaryTableUpdater      = 'automata/Sym12TernaryTableUpdater.cs'
src_SymBinaryTable                = 'automata/SymBinaryTable.cs'
src_SymBinaryTableUpdater         = 'automata/SymBinaryTableUpdater.cs'
src_TernaryTable                  = 'automata/TernaryTable.cs'
src_TernaryTableUpdater           = 'automata/TernaryTableUpdater.cs'
src_UnaryTable                    = 'automata/UnaryTable.cs'
src_UnaryTableUpdater             = 'automata/UnaryTableUpdater.cs'
src_ValueStore                    = 'automata/ValueStore.cs'
src_ValueStoreUpdater             = 'automata/ValueStoreUpdater.cs'

src_ForeignKeyCheckerBT           = 'automata/foreign-keys/ForeignKeyCheckerBT.cs'
src_ForeignKeyCheckerBU1          = 'automata/foreign-keys/ForeignKeyCheckerBU1.cs'
src_ForeignKeyCheckerBU2          = 'automata/foreign-keys/ForeignKeyCheckerBU2.cs'
src_ForeignKeyCheckerFCU          = 'automata/foreign-keys/ForeignKeyCheckerFCU.cs'
src_ForeignKeyCheckerICU          = 'automata/foreign-keys/ForeignKeyCheckerICU.cs'
src_ForeignKeyCheckerOCU          = 'automata/foreign-keys/ForeignKeyCheckerOCU.cs'
src_ForeignKeyCheckerSBST         = 'automata/foreign-keys/ForeignKeyCheckerSBST.cs'
src_ForeignKeyCheckerSBU          = 'automata/foreign-keys/ForeignKeyCheckerSBU.cs'
src_ForeignKeyCheckerST12U        = 'automata/foreign-keys/ForeignKeyCheckerST12U.cs'
src_ForeignKeyCheckerST3U         = 'automata/foreign-keys/ForeignKeyCheckerST3U.cs'
src_ForeignKeyCheckerSTSB         = 'automata/foreign-keys/ForeignKeyCheckerSTSB.cs'
src_ForeignKeyCheckerTB           = 'automata/foreign-keys/ForeignKeyCheckerTB.cs'
src_ForeignKeyCheckerTU1          = 'automata/foreign-keys/ForeignKeyCheckerTU1.cs'
src_ForeignKeyCheckerTU2          = 'automata/foreign-keys/ForeignKeyCheckerTU2.cs'
src_ForeignKeyCheckerTU3          = 'automata/foreign-keys/ForeignKeyCheckerTU3.cs'
src_ForeignKeyCheckerUB1          = 'automata/foreign-keys/ForeignKeyCheckerUB1.cs'
src_ForeignKeyCheckerUB2          = 'automata/foreign-keys/ForeignKeyCheckerUB2.cs'
src_ForeignKeyCheckerUSB          = 'automata/foreign-keys/ForeignKeyCheckerUSB.cs'
src_ForeignKeyCheckerUST12        = 'automata/foreign-keys/ForeignKeyCheckerUST12.cs'
src_ForeignKeyCheckerUST3         = 'automata/foreign-keys/ForeignKeyCheckerUST3.cs'
src_ForeignKeyCheckerUT1          = 'automata/foreign-keys/ForeignKeyCheckerUT1.cs'
src_ForeignKeyCheckerUT2          = 'automata/foreign-keys/ForeignKeyCheckerUT2.cs'
src_ForeignKeyCheckerUT3          = 'automata/foreign-keys/ForeignKeyCheckerUT3.cs'
src_ForeignKeyCheckerUU           = 'automata/foreign-keys/ForeignKeyCheckerUU.cs'
src_ForeignKeyViolationException  = 'automata/foreign-keys/ForeignKeyViolationException.cs'

src_AutoMisc                      = 'auto-utils/AutoMisc.cs'
src_AutoProcs                     = 'auto-utils/AutoProcs.cs'
src_IntCtrs                       = 'auto-utils/IntCtrs.cs'
src_Ints123                       = 'auto-utils/Ints123.cs'
src_Ints12                        = 'auto-utils/Ints12.cs'
src_Ints231                       = 'auto-utils/Ints231.cs'
src_Ints312                       = 'auto-utils/Ints312.cs'
src_PackedIntPairs                = 'auto-utils/PackedIntPairs.cs'
src_TableWriter                   = 'auto-utils/TableWriter.cs'


################################################################################

std_sources = [
  src_ArrayObjs,
  src_BinRelIter,
  src_BlankObj,
  src_Builder,
  src_EmptyRelObj,
  src_EmptySeqObj,
  src_FloatArrayObjs,
  src_FloatObj,
  src_IntArrayObjs,
  src_IntObj,
  src_NeBinRelObj,
  src_NeFloatSeqObj,
  src_NeIntSeqObj,
  src_NeSeqObj,
  src_NeSetObj,
  src_NeTernRelObj,
  src_NeTreeMapObj,
  src_NeTreeSetObj,
  src_NullObj,
  src_Obj,
  src_OptTagRecObj,
  src_Procs,
  src_RecordObj,
  src_SeqIter,
  src_SeqObj,
  src_SetIter,
  src_SymbObj,
  src_TaggedIntObj,
  src_TaggedObj,
  src_TernRelIter,

  src_AbstractULongSorter,
  src_Algs,
  src_Array,
  src_Canonical,
  src_CharStream,
  src_Conversions,
  src_DateTimeUtils,
  src_Debugging,
  src_ErrorHandling,
  src_Hashing,
  src_IO,
  src_Miscellanea,
  src_ObjPrinter,
  src_ObjVisitor,
  src_Parser,
  src_SymbTableFastCache,
  src_Tokenizer,
]

table_sources = [
  src_ArraySliceAllocator,
  src_BinaryTable,
  src_BinaryTableUpdater,
  src_ColumnBase,
  src_FloatColumn,
  src_FloatColumnUpdater,
  src_Index,
  src_IntColumn,
  src_IntColumnUpdater,
  src_IntStore,
  src_IntStoreUpdater,
  src_KeyViolationException,
  src_ObjColumn,
  src_ObjColumnUpdater,
  src_ObjStore,
  src_ObjStoreUpdater,
  src_OneWayBinTable,
  src_OverflowTable,
  src_RelAuto,
  src_Sym12TernaryTable,
  src_Sym12TernaryTableUpdater,
  src_SymBinaryTable,
  src_SymBinaryTableUpdater,
  src_TernaryTable,
  src_TernaryTableUpdater,
  src_UnaryTable,
  src_UnaryTableUpdater,
  src_ValueStore,
  src_ValueStoreUpdater,

  src_ForeignKeyCheckerBT,
  src_ForeignKeyCheckerBU1,
  src_ForeignKeyCheckerBU2,
  src_ForeignKeyCheckerFCU,
  src_ForeignKeyCheckerICU,
  src_ForeignKeyCheckerOCU,
  src_ForeignKeyCheckerSBST,
  src_ForeignKeyCheckerSBU,
  src_ForeignKeyCheckerST12U,
  src_ForeignKeyCheckerST3U,
  src_ForeignKeyCheckerSTSB,
  src_ForeignKeyCheckerTB,
  src_ForeignKeyCheckerTU1,
  src_ForeignKeyCheckerTU2,
  src_ForeignKeyCheckerTU3,
  src_ForeignKeyCheckerUB1,
  src_ForeignKeyCheckerUB2,
  src_ForeignKeyCheckerUSB,
  src_ForeignKeyCheckerUST12,
  src_ForeignKeyCheckerUST3,
  src_ForeignKeyCheckerUT1,
  src_ForeignKeyCheckerUT2,
  src_ForeignKeyCheckerUT3,
  src_ForeignKeyCheckerUU,
  src_ForeignKeyViolationException,

  src_AutoMisc,
  src_AutoProcs,
  src_IntCtrs,
  src_Ints123,
  src_Ints12,
  src_Ints231,
  src_Ints312,
  src_PackedIntPairs,
  src_TableWriter,
]

################################################################################

num_of_tabs = 0

def escape(ch):
  if ch == ord('\\'):
    return '\\\\'
  elif ch == ord('"'):
    return '\\"'
  elif ch >= ord(' ') or ch <= ord('~'):
    return chr(ch)
  elif ch == ord('\t'):
    global num_of_tabs
    num_of_tabs += 1
    return '\\t'
  else:
    print 'Invalid character: ' + ch
    exit(1);


def merge_lines(lines):
  merged_lines = []
  curr_line = ""
  for l in lines:
    if l:
      if len(curr_line) + len(l) > 2000:
        merged_lines.append(curr_line)
        curr_line = ""
      if curr_line:
        curr_line += "\\n"
      curr_line += l
  if curr_line:
    merged_lines.append(curr_line);
  return merged_lines


def convert_file(file_name, keep_all):
  lines = []
  f = open(file_name)
  past_header = False
  header = []
  for l in f:
    l = l.rstrip()
    uil = l.strip()
    if uil.startswith('Debug.Assert'):
      l = (len(l) - len(uil)) * ' ' + 'System.Diagnostics.' + uil
    past_header = past_header or (l != "" and not l.startswith('using '))
    if keep_all or past_header:
      el = ''.join([escape(ord(ch)) for ch in l])
      if past_header:
        lines.append(el)
      else:
        header.append(el)

  return ['"' + l + '"' for l in header + merge_lines(lines)]


# def to_code(bytes):
#   count = len(bytes)
#   ls = []
#   l = ' '
#   for i, b in enumerate(bytes):
#     l += ' ' + str(b) + (',' if i < count-1 else '')
#     if len(l) > 80:
#       ls.append(l)
#       l = ' '
#   if l:
#     ls.append(l)
#   return ls


def convert_files(directory, file_names, keep_all):
  ls = []
  for i, f in enumerate(file_names):
    if i > 0:
      ls.extend(['""', '""'])
    ls.extend(convert_file(directory + '/' + f, keep_all))
  return ['  ' + l for l in ls]


def data_array_def(array_name, directory, file_names, keep_all):
  lines = convert_files(directory, file_names, keep_all)
  # code = to_code(lines)
  if len(lines) <= 500:
    lines = [l + (',' if i < len(lines) - 1 else '') for i, l in enumerate(lines)]
    return ['String* ' + array_name + ' = ('] + lines + [');']
  code = []
  count = (len(lines) + 499) / 500;
  for i in range(count):
    code += ['String* ' + array_name + '_' + str(i) + ' = (']
    chunk = lines[500 * i : 500 * (i + 1)]
    code += [l + (',' if i < len(chunk) - 1 else '') for i, l in enumerate(chunk)]
    code += [');', '', '']
  pieces = [array_name + '_' + str(i) for i in range(count)]
  code += ['String* ' + array_name + ' = ' + ' & '.join(pieces) + ';']
  return code

################################################################################

from sys import argv, exit

if len(argv) != 3:
  print 'Usage: ' + argv[0] + ' <input directory> <output file>'
  exit(0)

_, input_dir, out_fname = argv

file_data = [
  data_array_def('core_runtime', input_dir, std_sources, False),
  data_array_def('table_runtime', input_dir, table_sources, False),
]

out_file = open(out_fname, 'w')
for i, f in enumerate(file_data):
  if i > 0:
    out_file.write('\n\n')
  for l in f:
    out_file.write(l + '\n');
