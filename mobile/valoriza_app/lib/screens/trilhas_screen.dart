import 'package:flutter/material.dart';
import '../services/api_service.dart';

class TrilhasScreen extends StatefulWidget {
  const TrilhasScreen({super.key});

  @override
  State<TrilhasScreen> createState() => _TrilhasScreenState();
}

class _TrilhasScreenState extends State<TrilhasScreen> {
  final _api = ApiService();
  List<dynamic> _trilhas = [];
  bool _carregando = true;

  @override
  void initState() {
    super.initState();
    _carregarTrilhas();
  }

  Future<void> _carregarTrilhas() async {
    try {
      final lista = await _api.getTrilhas();
      setState(() {
        _trilhas = lista;
        _carregando = false;
      });
    } catch (e) {
      setState(() => _carregando = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erro ao carregar: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Trilhas de Treinamento')),
      body: _carregando
          ? const Center(child: CircularProgressIndicator())
          : _trilhas.isEmpty
              ? const Center(child: Text('Nenhuma trilha disponível'))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _trilhas.length,
                  itemBuilder: (context, index) {
                    final trilha = _trilhas[index];
                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      child: ListTile(
                        title: Text(trilha['titulo'] ?? 'Sem título'),
                        subtitle: Text(trilha['descricao'] ?? ''),
                        trailing: Chip(
                          label: Text(trilha['nivel'] ?? 'Básico'),
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}
