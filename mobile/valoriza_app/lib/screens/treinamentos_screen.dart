import 'package:flutter/material.dart';
import '../services/api_service.dart';
import 'trilha_detalhe_screen.dart';

class TreinamentosScreen extends StatefulWidget {
  const TreinamentosScreen({super.key});
  @override
  State<TreinamentosScreen> createState() => _TreinamentosScreenState();
}

class _TreinamentosScreenState extends State<TreinamentosScreen> {
  final _api = ApiService();
  List<dynamic> _lista = [];
  bool _loading = true;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    setState(() { _loading = true; _erro = null; });
    try {
      _lista = await _api.getTrilhas();
    } catch (e) {
      _erro = e.toString();
    }
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Center(child: CircularProgressIndicator());
    if (_erro != null) return Center(child: Text(_erro!));
    if (_lista.isEmpty) return const Center(child: Text('Nenhuma trilha.'));

    return RefreshIndicator(
      onRefresh: _carregar,
      child: ListView.builder(
        padding: const EdgeInsets.all(12),
        itemCount: _lista.length,
        itemBuilder: (_, i) {
          final t = _lista[i] as Map<String, dynamic>;
          final id = t['id'] as int;
          final titulo = t['titulo']?.toString() ?? '';
          final carga = t['cargaHoraria'] ?? t['cargaHorariaMinutos'] ?? '';
          final qtd = t['quantidadeConteudos'] ?? t['conteudos']?.length ?? '';
          return Card(
            child: ListTile(
              title: Text(titulo),
              subtitle: Text('$carga min · $qtd conteúdos'),
              trailing: const Icon(Icons.chevron_right),
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => TrilhaDetalheScreen(trilhaId: id)),
                );
              },
            ),
          );
        },
      ),
    );
  }
}