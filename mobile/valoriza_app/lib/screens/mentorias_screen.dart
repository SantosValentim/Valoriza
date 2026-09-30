import 'package:flutter/material.dart';
import '../services/api_service.dart';

class MentoriasScreen extends StatefulWidget {
  const MentoriasScreen({super.key});
  @override
  State<MentoriasScreen> createState() => _MentoriasScreenState();
}

class _MentoriasScreenState extends State<MentoriasScreen> {
  List<dynamic> _lista = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      _lista = await ApiService().getMentorias();
    } catch (_) {}
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Center(child: CircularProgressIndicator());
    if (_lista.isEmpty) return const Center(child: Text('Nenhuma mentoria'));
    return ListView.builder(
      itemCount: _lista.length,
      itemBuilder: (_, i) {
        final m = _lista[i] as Map<String, dynamic>;
        return ListTile(
          title: Text(m['titulo']?.toString() ?? m['mentorNome']?.toString() ?? 'Mentoria'),
          subtitle: Text(m['status']?.toString() ?? ''),
        );
      },
    );
  }
}